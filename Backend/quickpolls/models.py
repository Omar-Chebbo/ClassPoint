# Backend/quickpolls/models.py

from django.db import models
from django.contrib.auth import get_user_model
import random
from students.models import Student 
User = get_user_model()

class QuickPoll(models.Model):
    QUESTION_TYPES = [
        ('true_false', 'True / False'),
        ('yes_no_unsure', 'Yes / No / Unsure'),
        ('custom', 'Custom'),
    ]
    name = models.CharField(max_length=120, db_index=True)
    code = models.CharField(max_length=6, unique=True)
    creator = models.ForeignKey(
        User,
        on_delete=models.CASCADE,
        related_name='quickpolls',
        null=True, blank=True      # ✅ make optional for anonymous
    )
    question_type = models.CharField(max_length=20, choices=QUESTION_TYPES)
    option_count = models.IntegerField(default=2)
    is_active = models.BooleanField(default=True)
    created_at = models.DateTimeField(auto_now_add=True)
    closed_at = models.DateTimeField(null=True, blank=True)

    def save(self, *args, **kwargs):
        if not self.code:
            self.code = self._generate_unique_code()
        super().save(*args, **kwargs)
        if not self.options.exists():
            self._create_default_options()

    def _generate_unique_code(self):
        while True:
            code = str(random.randint(1000, 9999))
            if not QuickPoll.objects.filter(code=code).exists():
                return code

    def _create_default_options(self):
        options = []
        if self.question_type == 'true_false':
            options = ['True', 'False']
        elif self.question_type == 'yes_no_unsure':
            options = ['Yes', 'No', 'Unsure']
        elif self.question_type == 'custom':
            options = [f'Option {i+1}' for i in range(self.option_count)]
        for text in options:
            PollOption.objects.create(poll=self, text=text)

    def __str__(self):
        return f"{self.name} ({self.code})"


class PollOption(models.Model):
    poll = models.ForeignKey(QuickPoll, on_delete=models.CASCADE, related_name='options')
    text = models.CharField(max_length=100)
    vote_count = models.IntegerField(default=0)

    def __str__(self):
        return f"{self.text} ({self.vote_count} votes)"
    
class PollVote(models.Model):
    poll = models.ForeignKey('QuickPoll', on_delete=models.CASCADE, related_name='votes')
    option = models.ForeignKey('PollOption', on_delete=models.CASCADE, related_name='votes')
    student = models.ForeignKey(Student, on_delete=models.CASCADE, related_name='poll_votes')
    voted_at = models.DateTimeField(auto_now_add=True)

    class Meta:
        unique_together = ('poll', 'student')  # ✅ one vote per student per poll

    def __str__(self):
        return f"{self.student.full_name} → {self.option.text}"


class Meta:
    unique_together = ('poll', 'student') # one vote per student per poll
    indexes = [
    models.Index(fields=['poll', 'student']),
    models.Index(fields=['poll', 'option']),
    ]


def __str__(self):

    return f"{self.student_id} → {self.poll.code} / {self.option_id}"