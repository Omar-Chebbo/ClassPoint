from rest_framework.routers import DefaultRouter
from django.urls import path
from .views import (
    StudentViewSet,
    StudentClassEnrollmentViewSet,
    StudentQuizSubmissionViewSet,
    StudentAnswerViewSet,
    JoinClassView,
    StudentQuizListView,
    StudentMultiQuizListView,
    StudentMultiQuizQuestionsView
)

router = DefaultRouter()
router.register('students', StudentViewSet, basename='student')
router.register('enrollments', StudentClassEnrollmentViewSet, basename='enrollment')
router.register('submissions', StudentQuizSubmissionViewSet, basename='submission')
router.register('answers', StudentAnswerViewSet, basename='answer')

urlpatterns = [
    path('join/', JoinClassView.as_view(), name='join-class'),
    
    # Standalone quizzes
    path('quizzes/', StudentQuizListView.as_view(), name='student-quiz-list'),
    ##path('login/', StudentLoginView.as_view(), name='student-login'),
    
    # Multi-quiz endpoints
    path('multi-quiz/', StudentMultiQuizListView.as_view(), name='student-multi-quiz-list'),
    path('multi-quiz/<uuid:multi_question_id>/', StudentMultiQuizQuestionsView.as_view(), name='student-multi-quiz-questions'),
    
    # Direct list/retrieve for students at /api/students/
    path('', StudentViewSet.as_view({'get': 'list'}), name='student-list-root'),
    path('<int:pk>/', StudentViewSet.as_view({'get': 'retrieve'}), name='student-detail-root'),
]

urlpatterns += router.urls
