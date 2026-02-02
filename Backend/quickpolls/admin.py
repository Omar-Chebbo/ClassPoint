from django.contrib import admin
from .models import QuickPoll, PollOption,PollVote


class PollOptionInline(admin.TabularInline):
    model = PollOption
    extra = 0


@admin.register(QuickPoll)
class QuickPollAdmin(admin.ModelAdmin):
    list_display = ('code', 'creator', 'question_type', 'is_active', 'created_at')
    list_filter = ('is_active', 'question_type')
    search_fields = ('code',)
    inlines = [PollOptionInline]


@admin.register(PollOption)
class PollOptionAdmin(admin.ModelAdmin):
    list_display = ('poll', 'text', 'vote_count')
    search_fields = ('text',)

    
@admin.register(PollVote)
class PollVoteAdmin(admin.ModelAdmin):
    list_display = ('id', 'poll', 'option', 'student', 'voted_at')
    list_filter = ('poll',)
    search_fields = ('poll__code', 'student__full_name', 'student__email')
    list_select_related = ('poll', 'option', 'student')