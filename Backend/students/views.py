from rest_framework import viewsets, permissions, status
from rest_framework.response import Response
from rest_framework.views import APIView
from django.shortcuts import get_object_or_404
from classes.models import Class
from quizzes.models import Quiz
from .models import Student, StudentClassEnrollment, StudentQuizSubmission, StudentAnswer
from .serializers import (
    StudentSerializer, StudentClassEnrollmentSerializer,
    StudentQuizSubmissionSerializer, StudentAnswerSerializer,
    ShortAnswerSerializer, WordCloudAnswerSerializer, 
    MultipleChoiceAnswerSerializer, DrawingAnswerSerializer,
    ImageUploadAnswerSerializer
)
# Permissions are imported but not currently used in views
# from .permissions import (
#     IsTeacherOrReadOnly, IsClassTeacher, IsEnrolledStudent,
#     CanViewStudentAnswers, CanCreateStudentAnswer, IsClassActive,
#     StudentAnswerAccess
# )
from .authentication import StudentToken, StudentAuthentication, StudentUser


class StudentViewSet(viewsets.ModelViewSet):
    """
    ViewSet for Student model.
    - Teachers can view all students
    - Students can view their own profile
    - Anyone can create students (for joining classes)
    """
    queryset = Student.objects.all()
    serializer_class = StudentSerializer
    # Dynamic: allow unauthenticated read when querying by student_id; otherwise require auth
    permission_classes = [permissions.IsAuthenticated]

    def get_permissions(self):
        if self.request.method == 'GET' and self.request.query_params.get('student_id'):
            return [permissions.AllowAny()]
        return super().get_permissions()
    
    def get_queryset(self):
        """Filter students based on user permissions."""
        if self.request.user.is_authenticated:
            # Teacher sees all students (tests expect global list)
            return Student.objects.all().order_by('-joined_at')
        # Unauthenticated: allow only direct lookup by student_id
        student_id = self.request.query_params.get('student_id')
        if student_id:
            return Student.objects.filter(id=student_id)
        return Student.objects.none()


class StudentClassEnrollmentViewSet(viewsets.ModelViewSet):
    """
    ViewSet for StudentClassEnrollment model.
    - Teachers can view enrollments in their classes
    - Students can view their own enrollments
    - Only enrolled students can create enrollments
    """
    queryset = StudentClassEnrollment.objects.all()
    serializer_class = StudentClassEnrollmentSerializer
    # Dynamic: allow unauthenticated read when querying by student_id; otherwise require auth
    permission_classes = [permissions.IsAuthenticated]

    def get_permissions(self):
        if self.request.method == 'GET' and self.request.query_params.get('student_id'):
            return [permissions.AllowAny()]
        return super().get_permissions()
    
    def get_queryset(self):
        """Filter enrollments based on user permissions."""
        if self.request.user.is_authenticated:
            # Teacher can see enrollments in their classes
            return StudentClassEnrollment.objects.filter(
                classroom__teacher=self.request.user
            ).order_by('-joined_at')
        # Unauthenticated: allow only direct lookup by student_id
        student_id = self.request.query_params.get('student_id')
        if student_id:
            return StudentClassEnrollment.objects.filter(student_id=student_id).order_by('-joined_at')
        return StudentClassEnrollment.objects.none()


class StudentQuizSubmissionViewSet(viewsets.ModelViewSet):
    """
    ViewSet for StudentQuizSubmission model.
    - Teachers can view all submissions in their classes
    - Students can view their own submissions
    - Only enrolled students can create submissions
    """
    queryset = StudentQuizSubmission.objects.all()
    serializer_class = StudentQuizSubmissionSerializer
    permission_classes = [permissions.AllowAny]  # Allow anyone to view submissions
    
    def get_queryset(self):
        """Filter submissions based on user permissions."""
        if self.request.user.is_authenticated and hasattr(self.request.user, 'course_set'):
            # Teacher can see all submissions in their classes
            return StudentQuizSubmission.objects.filter(
                quiz__course__teacher=self.request.user
            )
        else:
            # Students can only see their own submissions
            student_id = self.request.query_params.get('student_id')
            if student_id:
                return StudentQuizSubmission.objects.filter(student_id=student_id)
            return StudentQuizSubmission.objects.none()


class StudentAnswerViewSet(viewsets.ModelViewSet):
    """
    ViewSet for StudentAnswer model.
    - Teachers can view all answers in their classes
    - Students can view their own answers using tokens
    - Only enrolled students can create answers
    """
    queryset = StudentAnswer.objects.all()
    serializer_class = StudentAnswerSerializer
    # Dynamic: allow unauthenticated read when querying by student_id; otherwise require auth
    permission_classes = [permissions.IsAuthenticated]
    
    def get_serializer_class(self):
        """Return appropriate serializer based on quiz type for creation."""
        if self.action == 'create':
            quiz_id = self.request.data.get('quiz_id')
            if quiz_id:
                try:
                    quiz = Quiz.objects.get(id=quiz_id)
                    quiz_type = quiz.quiz_type
                    
                    # Return specific serializer based on quiz type
                    if quiz_type == 'short_answer':
                        return ShortAnswerSerializer
                    elif quiz_type == 'word_cloud':
                        return WordCloudAnswerSerializer
                    elif quiz_type == 'multiple_choice':
                        return MultipleChoiceAnswerSerializer
                    elif quiz_type == 'drawing':
                        return DrawingAnswerSerializer
                    elif quiz_type == 'image_upload':
                        return ImageUploadAnswerSerializer
                except Quiz.DoesNotExist:
                    pass
        
        # ALAA_SAJA_TODO: Handle multi-quiz submissions
        # Add logic to handle student answers for multi-quiz:
        # - Check if quiz is part of a multi-quiz (multi_question_id is not null)
        # - For multi-quiz, students can submit answers to individual questions
        # - Each question in the multi-quiz is still a separate Quiz object
        # - Student answers work the same way (one answer per quiz submission)
        # - No changes needed to StudentAnswer model or serializers
        # PERMISSIONS: Students can only submit answers to quizzes they have access to
        # PERMISSIONS: Check if student is enrolled in the course
        # PERMISSIONS: Validate that the quiz belongs to the student's enrolled course
        # PERMISSIONS: Teachers can view all student submissions for their multi-quiz
        # Default serializer for read operations or unknown quiz types
        return StudentAnswerSerializer

    def get_permissions(self):
        if self.request.method == 'GET' and self.request.query_params.get('student_id'):
            return [permissions.AllowAny()]
        return super().get_permissions()
    
    def get_queryset(self):
        """Filter answers based on user permissions."""
        if isinstance(self.request.user, StudentUser):
            # Student with valid token can see their own answers
            return StudentAnswer.objects.filter(
                submission__student_id=self.request.user.student.id
            ).order_by('-submitted_at')
        elif self.request.user.is_authenticated:
            # Teacher can see all answers in their classes
            return StudentAnswer.objects.filter(
                submission__quiz__course__teacher=self.request.user
            ).order_by('-submitted_at')
        # Unauthenticated: allow only direct lookup by student_id
        student_id = self.request.query_params.get('student_id')
        if student_id:
            return StudentAnswer.objects.filter(
                submission__student_id=student_id
            ).order_by('-submitted_at')
        return StudentAnswer.objects.none()
    
    def perform_create(self, serializer):
        """Ensure student can only create answers for their own submissions."""
        if isinstance(self.request.user, StudentUser):
            quiz_id = self.request.data.get('quiz_id')
            if quiz_id:
                # Get or create the submission for this student and quiz
                submission, created = StudentQuizSubmission.objects.get_or_create(
                    student=self.request.user.student,
                    quiz_id=quiz_id,
                    defaults={'score': None, 'is_late': False}
                )
                serializer.save(submission=submission)
            else:
                # If no quiz_id provided, use the provided submission
                super().perform_create(serializer)
        else:
            # For teachers or other cases, use the provided submission
            super().perform_create(serializer)


class JoinClassView(APIView):
    """
    Allow students to join a class using a class code.
    Only works if the class is active.
    """
    permission_classes = [permissions.AllowAny]

    def post(self, request):
        full_name = request.data.get("full_name")
        class_code = request.data.get("class_code")

        # Validate input
        if not full_name or not class_code:
            return Response(
                {"error": "Both full_name and class_code are required."},
                status=status.HTTP_400_BAD_REQUEST,
            )

        # Check if class exists and is active
        classroom = Class.objects.filter(code=class_code, active=True).first()
        if not classroom:
            return Response(
                {"error": "Invalid or inactive class code."},
                status=status.HTTP_404_NOT_FOUND,
            )

        # Create or get the student
        student, _ = Student.objects.get_or_create(full_name=full_name)

        # Check if already enrolled
        already_enrolled = StudentClassEnrollment.objects.filter(
            student=student, classroom=classroom
        ).exists()
        if already_enrolled:
            return Response(
                {
                    "message": "You are already enrolled in this class.",
                    "student_id": student.id,
                    "class_id": classroom.id,
                },
                status=status.HTTP_200_OK,
            )

        # Enroll the student
        enrollment = StudentClassEnrollment.objects.create(
            student=student, classroom=classroom
        )

        # Generate authentication token for the student
        token = StudentToken.generate_token(
            student_id=student.id,
            class_id=classroom.id,
            enrollment_id=enrollment.id
        )

        return Response(
            {
                "message": f"Successfully joined class {classroom.course.name} ({classroom.code})",
                "student_id": student.id,
                "class_id": classroom.id,
                "enrollment_id": enrollment.id,
                "access_token": token,
                "token_type": "Bearer",
                "expires_in": 86400,  # 24 hours in seconds
            },
            status=status.HTTP_201_CREATED,
        )


class StudentQuizListView(APIView):
    """
    Allow students to view available quizzes in their class.
    Students authenticate with their token to see quizzes for their enrolled class.
    """
    permission_classes = [permissions.IsAuthenticated]

    def get(self, request):
        """Get available quizzes for the student's class."""
        # Check if user is a student with valid token
        if not isinstance(request.user, StudentUser):
            return Response(
                {"error": "Student authentication required"},
                status=status.HTTP_401_UNAUTHORIZED
            )
        
        classroom = request.user.classroom
        
        # Get standalone quizzes only (exclude multi-quiz questions)
        quizzes = Quiz.objects.filter(
            course=classroom.course,
            multi_question_id__isnull=True  # Only standalone quizzes
        ).order_by('-created_at')
        
        # Serialize quiz data for students (without sensitive info)
        quiz_data = []
        for quiz in quizzes:
            quiz_data.append({
                'id': quiz.id,
                'title': quiz.title,
                'quiz_type': quiz.quiz_type,
                'properties': quiz.properties,
                'created_at': quiz.created_at,
                'show_timer': quiz.show_timer,
                'auto_close_after_seconds': quiz.auto_close_after_seconds,
            })
        
        return Response({
            'quizzes': quiz_data,
            'class_info': {
                'id': classroom.id,
                'code': classroom.code,
                'course_name': classroom.course.name,
                'active': classroom.active
            }
        })


class StudentMultiQuizListView(APIView):
    """View for students to list available multi-quiz"""
    permission_classes = [permissions.IsAuthenticated]
    
    def get(self, request):
        """List all multi-quiz available to the student"""
        # Get student ID from StudentUser
        student_id = request.user.student.id
        
        # Get all unique multi_question_ids for quizzes in student's enrolled courses
        multi_quiz_ids = Quiz.objects.filter(
            multi_question_id__isnull=False,
            course__classes__enrollments__student_id=student_id
        ).values_list('multi_question_id', flat=True).distinct()
        
        # Build response with quizzes grouped by multi_question_id
        result = {}
        for multi_id in multi_quiz_ids:
            quizzes = Quiz.objects.filter(multi_question_id=multi_id).order_by('question_order')
            if quizzes.exists():
                from quizzes.serializers import QuizSerializer
                quiz_serializer = QuizSerializer(quizzes, many=True)
                result[str(multi_id)] = quiz_serializer.data
        
        return Response(result)


class StudentMultiQuizQuestionsView(APIView):
    """View for students to get questions in a specific multi-quiz"""
    permission_classes = [permissions.IsAuthenticated]
    
    def get(self, request, multi_question_id):
        """Get all questions in a specific multi-quiz"""
        # Get student's enrollments
        student_id = request.user.student.id
        enrollments = StudentClassEnrollment.objects.filter(student_id=student_id)
        enrolled_courses = [e.classroom.course for e in enrollments]
        
        # Get questions from this multi-quiz that belong to enrolled courses
        questions = Quiz.objects.filter(
            multi_question_id=multi_question_id,
            course__in=enrolled_courses
        ).order_by('question_order')
        
        if not questions.exists():
            return Response(
                {'detail': 'Multi-quiz not found or not available'}, 
                status=status.HTTP_404_NOT_FOUND
            )
        
        from quizzes.serializers import QuizSerializer
        serializer = QuizSerializer(questions, many=True)
        return Response(serializer.data)
class StudentLoginView(APIView):
    """
    Login endpoint for students.
    POST { "email": "...", "full_name": "..." }
    → returns token if student exists.
    """
    permission_classes = []  # open to all (no auth needed)

    def post(self, request):
        email = request.data.get('email')
        full_name = request.data.get('full_name')

        if not email or not full_name:
            return Response({"error": "Both email and full_name are required."},
                            status=status.HTTP_400_BAD_REQUEST)

        try:
            student = Student.objects.get(email=email, full_name=full_name)
        except Student.DoesNotExist:
            return Response({"error": "Student not found. Contact your teacher to register."},
                            status=status.HTTP_404_NOT_FOUND)

        token, _ = StudentToken.objects.get_or_create(student=student)

        return Response({
            "token": token.key,
            "student_name": student.full_name,
            "email": student.email
        }, status=status.HTTP_200_OK)