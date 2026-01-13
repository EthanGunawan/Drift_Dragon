using System;
using System.Collections.Generic;
using System.Linq;
using Drift_Dragon.BusinessLogic;
using Microsoft.Maui.Controls;

namespace Drift_Dragon
{
    public partial class MoodJournalPage : ContentPage
    {
        private readonly MoodJournalManager _moodManager = new();

        private MoodJournal? _selectedEntryForEdit;
        private List<MoodJournal> _recentEntries = new();

        public MoodJournalPage()
        {
            InitializeComponent();

            MoodSlider.ValueChanged += MoodSlider_ValueChanged;

            MoodSlider.Value = 2; // Default "Ok"
            UpdateEmoji();
            LoadHistory();
        }

        private void MoodSlider_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            UpdateEmoji();
        }

        private void UpdateEmoji()
        {
            int moodValue = (int)MoodSlider.Value;
            MoodEmojiLabel.Text = moodValue switch
            {
                0 => "😢",  // Terrible
                1 => "🙁",  // Bad
                2 => "😐",  // Ok
                3 => "🙂",  // Good
                4 => "😊",  // Great
                _ => "😐"
            };
        }

        private async void OnSaveMoodClicked(object sender, EventArgs e)
        {
            int moodValue = (int)MoodSlider.Value;
            Mood mood = (Mood)moodValue;
            string reflection = ReflectionEntry.Text ?? string.Empty;

            if (_selectedEntryForEdit == null)
            {
                // New entry
                await _moodManager.AddEntryAsync(mood, reflection);
            }
            else
            {
                // Update existing
                _selectedEntryForEdit.Mood = mood;
                _selectedEntryForEdit.Reflection = reflection;
                await _moodManager.UpdateEntryAsync(_selectedEntryForEdit);
                _selectedEntryForEdit = null;
            }

            // Reset form
            MoodSlider.Value = 2;
            ReflectionEntry.Text = string.Empty;
            UpdateEmoji();

            LoadHistory();
            await DisplayAlert("Saved! ✨", "Your mood is logged.", "OK");
        }

        private async void LoadHistory()
        {
            _recentEntries = await _moodManager.GetRecentAsync(10);

            MoodHistoryCollectionView.ItemsSource = _recentEntries
                .Select(j => new
                {
                    j.MoodJournalID,
                    j.Date,
                    j.Reflection,
                    Emoji = GetEmoji(j.Mood),
                    MoodScore = $"{(int)j.Mood}/4"
                })
                .ToList();
        }

        private string GetEmoji(Mood mood) => mood switch
        {
            Mood.Terrible => "😢",
            Mood.Bad => "🙁",
            Mood.Ok => "😐",
            Mood.Good => "🙂",
            Mood.Great => "😊",
            _ => "😐"
        };

        private void MoodHistoryCollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = e.CurrentSelection.FirstOrDefault();
            if (selected == null)
            {
                _selectedEntryForEdit = null;
                return;
            }

            // Anonymous type: grab the ID via reflection
            var idProp = selected.GetType().GetProperty("MoodJournalID");
            if (idProp == null)
            {
                _selectedEntryForEdit = null;
                return;
            }

            var id = (int)idProp.GetValue(selected)!;
            _selectedEntryForEdit = _recentEntries.FirstOrDefault(x => x.MoodJournalID == id);
        }

        private void OnEditSelectedClicked(object sender, EventArgs e)
        {
            if (_selectedEntryForEdit == null)
                return;

            MoodSlider.Value = (int)_selectedEntryForEdit.Mood;
            ReflectionEntry.Text = _selectedEntryForEdit.Reflection;
            UpdateEmoji();
        }

        private async void OnDeleteSelectedClicked(object sender, EventArgs e)
        {
            if (_selectedEntryForEdit == null)
            {
                await DisplayAlert("Nothing selected", "Tap a mood entry first.", "OK");
                return;
            }

            bool confirm = await DisplayAlert(
                "Delete entry",
                "Are you sure you want to delete this mood journal entry?",
                "Delete",
                "Cancel");

            if (!confirm)
                return;

            await _moodManager.DeleteEntryAsync(_selectedEntryForEdit.MoodJournalID);
            _selectedEntryForEdit = null;

            // Clear form back to neutral if it was showing that entry
            MoodSlider.Value = 2;
            ReflectionEntry.Text = string.Empty;
            UpdateEmoji();

            LoadHistory();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (MoodHistoryCollectionView.ItemsSource == null)
            {
                LoadHistory();
            }
        }
    }
}
