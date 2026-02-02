from django.urls import reverse
from rest_framework.test import APIClient
from django.test import TestCase
from students.models import Student
from quickpolls.models import QuickPoll, PollOption, PollVote

class VoteFlowTests(TestCase):
    def setUp(self):
        self.client = APIClient()
        # create teacher/user, poll, options, and a student + token based on your auth scheme
        # TODO: bootstrap StudentToken so client is authenticated as the student

    def test_student_can_vote_once(self):
        # self.client.credentials(HTTP_AUTHORIZATION=f'Token {token}')
        # r = self.client.post(f'/api/quickpolls/{poll.code}/vote/', {'option_id': opt.id}, format='json')
        # self.assertEqual(r.status_code, 200)
        # self.assertEqual(PollVote.objects.filter(poll=poll, student=student).count(), 1)
        pass