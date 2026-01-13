using System;
using System.Collections.Generic;
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
            if (moodValue == 0)
                MoodEmojiLabel.Text = "😢";  // Terrible
            else if (moodValue == 1)
                MoodEmojiLabel.Text = "🙁";  // Bad
            else if (moodValue == 2)
                MoodEmojiLabel.Text = "😐";  // Ok
            else if (moodValue == 3)
                MoodEmojiLabel.Text = "🙂";  // Good
            else if (moodValue == 4)
                MoodEmojiLabel.Text = "😊";  // Great
            else
                MoodEmojiLabel.Text = "😐";
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

            // Build a simple list for the CollectionView without LINQ
            var items = new List<object>();
            foreach (var j in _recentEntries)
            {
                items.Add(new
                {
                    MoodJournalID = j.MoodJournalID,
                    Date = j.Date,
                    Reflection = j.Reflection,
                    Emoji = GetEmoji(j.Mood),
                    MoodScore = ((int)j.Mood).ToString() + "/4"
                });
            }

            MoodHistoryCollectionView.ItemsSource = items;
        }

        private string GetEmoji(Mood mood)
        {
            if (mood == Mood.Terrible) return "😢";
            if (mood == Mood.Bad)      return "🙁";
            if (mood == Mood.Ok)       return "😐";
            if (mood == Mood.Good)     return "🙂";
            if (mood == Mood.Great)    return "😊";
            return "😐";
        }

        private void MoodHistoryCollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Get the selected anonymous object
            object selected = null;
            foreach (var item in e.CurrentSelection)
            {
                selected = item;
                break;
            }

            if (selected == null)
            {
                _selectedEntryForEdit = null;
                return;
            }

            // Read MoodJournalID from the anonymous object with reflection,
            // but without LINQ or fancy helpers
            var type = selected.GetType();
            var idProp = type.GetProperty("MoodJournalID");
            if (idProp == null)
            {
                _selectedEntryForEdit = null;
                return;
            }

            var value = idProp.GetValue(selected);
            if (value == null)
            {
                _selectedEntryForEdit = null;
                return;
            }

            int id = (int)value;

            // Find the real MoodJournal in _recentEntries using a simple loop
            _selectedEntryForEdit = null;
            foreach (var j in _recentEntries)
            {
                if (j.MoodJournalID == id)
                {
                    _selectedEntryForEdit = j;
                    break;
                }
            }
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

            // Clear form back to neutral
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
