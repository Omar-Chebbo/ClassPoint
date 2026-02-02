from django.urls import path
from .views import (
    CreateQuickPollView,
    SubmitVoteView,
    PollResultsView,
    ClosePollView,
    PollsByNameView,
    PollResultsByNameView,
    get_poll_details,
)

urlpatterns = [
    path('create/', CreateQuickPollView.as_view(), name='create_quickpoll'),
    path('<str:code>/vote/', SubmitVoteView.as_view(), name='submit_vote'),
    path('<str:code>/results/', PollResultsView.as_view(), name='poll_results'),
    path('<str:code>/close/', ClosePollView.as_view(), name='close_poll'),
    path("name/<str:name>/", PollResultsByNameView.as_view(), name="polls_by_name"), 
    path('<str:code>/', get_poll_details, name='poll_details'),



]
