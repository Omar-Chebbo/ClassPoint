from rest_framework import serializers
from .models import QuickPoll, PollOption, PollVote
from students.models import Student
from django.db.models import F


class PollOptionSerializer(serializers.ModelSerializer):
    class Meta:
        model = PollOption
        fields = ['id', 'text', 'vote_count']


class QuickPollSerializer(serializers.ModelSerializer):
    options = PollOptionSerializer(many=True, read_only=True)
    creator = serializers.StringRelatedField(read_only=True)

    class Meta:
        model = QuickPoll
        fields = [
            'id','name', 'code', 'creator', 'question_type', 'option_count',
            'is_active', 'created_at', 'closed_at', 'options'
        ]
        read_only_fields = ['code', 'created_at', 'closed_at', 'options']

    def create(self, validated_data):
        user = self.context['request'].user
        if user.is_anonymous:  # ✅ allow anonymous poll creation
            user = None
        poll = QuickPoll.objects.create(creator=user, **validated_data)
        return poll


# ✅ Updated VoteSerializer to support student info and unique voting
class VoteSerializer(serializers.Serializer):
    option_id = serializers.IntegerField()
    student_name = serializers.CharField(write_only=True)
    student_email = serializers.EmailField(write_only=True)

    def validate(self, attrs):
        poll = self.context['poll']
        option_id = attrs.get('option_id')
        student_name = attrs.get('student_name')
        student_email = attrs.get('student_email')

        # Ensure option belongs to poll
        try:
            option = PollOption.objects.get(id=option_id, poll=poll)
        except PollOption.DoesNotExist:
            raise serializers.ValidationError('Invalid option for this poll.')

        # Ensure poll is active
        if not poll.is_active:
            raise serializers.ValidationError('This poll is closed.')

        # ✅ Require the student to already exist
        try:
            student = Student.objects.get(email=student_email, full_name=student_name)
        except Student.DoesNotExist:
            raise serializers.ValidationError({
                "detail": "Invalid name or email. Please use your registered student credentials."
            })

        # Ensure student hasn’t already voted
        if PollVote.objects.filter(poll=poll, student=student).exists():
            raise serializers.ValidationError('You have already voted in this poll.')

        attrs['option'] = option
        attrs['student'] = student
        return attrs


    def create(self, validated_data):
        option = validated_data['option']
        student = validated_data['student']
        poll = self.context['poll']

        vote = PollVote.objects.create(
            poll=poll,
            option=option,
            student=student
        )

        # Increment vote count safely
        option.vote_count = F('vote_count') + 1
        option.save(update_fields=['vote_count'])


        return vote
