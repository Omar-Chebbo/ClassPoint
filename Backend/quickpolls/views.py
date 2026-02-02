# Backend/quickpolls/views.py

from rest_framework import generics, status
from rest_framework.response import Response
from rest_framework.permissions import AllowAny
from rest_framework.views import APIView
from rest_framework.decorators import api_view, permission_classes

from django.db import transaction
from django.db.models import F
from django.shortcuts import get_object_or_404
from .models import QuickPoll, PollOption,PollVote
from .serializers import QuickPollSerializer, PollOptionSerializer
from students.authentication import StudentAuthentication
from rest_framework.permissions import IsAuthenticated
from .serializers import VoteSerializer





class CreateQuickPollView(generics.CreateAPIView):
    """Create a new poll (e.g. True/False, Yes/No/Unsure, custom)"""
    serializer_class = QuickPollSerializer
    permission_classes = [AllowAny]


from rest_framework.permissions import AllowAny
from students.models import Student

class SubmitVoteView(APIView):
    permission_classes = [AllowAny]

    def post(self, request, code):
        try:
            poll = QuickPoll.objects.get(code=code, is_active=True)
        except QuickPoll.DoesNotExist:
            return Response({"error": "Poll not found."}, status=404)

        option_id      = request.data.get("option_id")
        student_name   = (request.data.get("student_name") or "").strip()
        student_email  = (request.data.get("student_email") or "").strip()

        if not option_id or not student_name or not student_email:
            return Response(
                {"detail": "Name, email and option are required."},
                status=400,
            )

        # 1️⃣ Find student by email (and optionally name)
        student = Student.objects.filter(
            email__iexact=student_email,
            full_name__iexact=student_name,
        ).order_by("id").first()

        if not student:
            return Response(
                {"detail": "You are not registered as a student."},
                status=403,
            )

        # 2️⃣ Check that option belongs to this poll
        try:
            option = poll.options.get(id=option_id)
        except PollOption.DoesNotExist:
            return Response({"detail": "Invalid option."}, status=400)

        # 3️⃣ Prevent duplicate vote by *student*
        if PollVote.objects.filter(poll=poll, student=student).exists():
            return Response(
                {"error": "You have already voted in this poll."},
                status=409,
            )

        # 4️⃣ Create the vote
        PollVote.objects.create(
            poll=poll,
            option=option,
            student=student,
        )

        # 5️⃣ Update the option vote count
        option.vote_count = option.votes.count()
        option.save()


        return Response({"message": "Vote submitted successfully!"})


class PollResultsView(generics.RetrieveAPIView):
    permission_classes = [AllowAny]

    def get(self, request, code):
        try:
            poll = QuickPoll.objects.get(code=code)
        except QuickPoll.DoesNotExist:
            return Response({"error": "Poll not found."}, status=404)

        options_data = []
        for option in poll.options.all():
            voters = option.votes.select_related('student').values_list('student__full_name', flat=True)

            options_data.append({
                "text": option.text,            # <-- frontend expects "text"
                "count": option.vote_count,     # <-- frontend expects "count"
                "voters": list(voters)
            })

        return Response({
            "poll_code": poll.code,
            "name": poll.name,
            "question_type": poll.question_type,
            "options": options_data            # <-- IMPORTANT: frontend searches for "options"
        })

class PollsByNameView(APIView):
    def get(self, request, name):
        polls = QuickPoll.objects.filter(name=name)
        if not polls.exists():
            return Response({"error": "No polls found with that name."}, status=status.HTTP_404_NOT_FOUND)

        all_results = []
        for poll in polls:
            poll_data = {
                "poll_code": poll.code,
                "created_at": poll.created_at,
                "results": []
            }
            for option in poll.options.all():
                voters = option.votes.select_related('student').values_list('student__full_name', flat=True)
                poll_data["results"].append({
                    "option": option.text,
                    "vote_count": option.vote_count,
                    "voters": list(voters),
                })
            all_results.append(poll_data)

        return Response({
            "poll_name": name,
            "polls": all_results
        })



class ClosePollView(APIView):
    """Close a poll by its code"""
    permission_classes = [AllowAny]

    def post(self, request, code):
        poll = get_object_or_404(QuickPoll, code=code)
        poll.is_active = False
        poll.save()
        return Response({"message": "Poll closed successfully."})


class PollResultsByNameView(APIView):
    permission_classes = [AllowAny]

    def get(self, request, name):
        # ✅ Case-insensitive and partial match
        polls = QuickPoll.objects.filter(name__icontains=name).order_by('-created_at')
        if not polls.exists():
            return Response({"polls": []}, status=status.HTTP_200_OK)

        data = []
        for poll in polls:
            results = []
            for option in poll.options.all():
                voters = option.votes.select_related('student').values_list('student__full_name', flat=True)
                results.append({
                    "option": option.text,
                    "vote_count": option.vote_count,
                    "voters": list(voters),
                })

            data.append({
                "poll_code": poll.code,
                "poll_name": poll.name,  # ✅ include the actual name
                "created_at": poll.created_at.strftime("%Y-%m-%d %H:%M:%S"),
                "results": results,
            })

        return Response({
            "search_query": name,
            "polls": data
        })
    

@api_view(["GET"])
@permission_classes([AllowAny])
def get_poll_details(request, code):
    try:
        poll = QuickPoll.objects.get(code=code, is_active=True)
    except QuickPoll.DoesNotExist:
        return Response({"error": "Poll not found or inactive."}, status=404)

    options = poll.options.all().values("id", "text")  # id + text for your frontend

    return Response({
        "poll_code": poll.code,
        "name": poll.name,
        "question_type": poll.question_type,
        "options": list(options),
    })
