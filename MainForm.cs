using System;
using System.Linq;
using System.Windows.Forms;
using SchedulerApp.Data;
using SchedulerApp.Models;
using System.Drawing;

namespace SchedulerApp
{
    public class MainForm : Form
    {
        private TextBox titleBox;
        private DateTimePicker datePicker;
        private ComboBox hourBox;
        private ComboBox minuteBox;
        private MonthCalendar calendar;
        private ListBox eventList;
        private Button addButton;
        private Button deleteButton;
        private ToolStrip toolStrip;
        private ToolStripButton homeButton, addEventButton, deleteEventButton, toggleThemeButton;
        private Panel leftPanel;
        private Panel rightPanel;
        private Label headerLabel, titleLabel, dateLabel, timeLabel, eventListLabel;

        private AppDbContext _db;
        private bool isDark = true;

        public MainForm()
        {
            Text = "Aditya's Scheduler";
            Width = 950;
            Height = 610;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;

            InitializeControls();
            ApplyTheme();

            _db = new AppDbContext();
            _db.Database.EnsureCreated();
            LoadEventsForDate(DateTime.Today);
        }

        private void InitializeControls()
        {
            // ----- Toolbar -----
            toolStrip = new ToolStrip();
            toolStrip.GripStyle = ToolStripGripStyle.Hidden;
            toolStrip.Dock = DockStyle.Top;

            homeButton = new ToolStripButton("🏠 Home");
            addEventButton = new ToolStripButton("➕ Add");
            deleteEventButton = new ToolStripButton("🗑️ Delete");
            toggleThemeButton = new ToolStripButton("🌙 Dark Mode");

            toolStrip.Items.AddRange(new ToolStripItem[] {
                homeButton, addEventButton, deleteEventButton, new ToolStripSeparator(), toggleThemeButton
            });

            Controls.Add(toolStrip);

            // ----- Panels -----
            leftPanel = new Panel { Left = 0, Top = toolStrip.Height, Width = 320, Height = ClientSize.Height - toolStrip.Height };
            rightPanel = new Panel { Left = 330, Top = toolStrip.Height, Width = 590, Height = ClientSize.Height - toolStrip.Height };

            headerLabel = new Label { Text = "📅 Schedule Event", Font = new Font("Segoe UI", 12, FontStyle.Bold), Top = 20, Left = 20, AutoSize = true };
            titleLabel = new Label { Text = "Title:", Top = 60, Left = 20, Width = 100 };
            titleBox = new TextBox { Top = 85, Left = 20, Width = 260 };

            dateLabel = new Label { Text = "Date:", Top = 125, Left = 20, Width = 100 };
            datePicker = new DateTimePicker { Top = 150, Left = 20, Width = 260, Format = DateTimePickerFormat.Short };

            timeLabel = new Label { Text = "Time (HH:MM):", Top = 190, Left = 20, Width = 150 };
            hourBox = new ComboBox { Top = 215, Left = 20, Width = 60, DropDownStyle = ComboBoxStyle.DropDownList };
            minuteBox = new ComboBox { Top = 215, Left = 90, Width = 60, DropDownStyle = ComboBoxStyle.DropDownList };
            for (int h = 0; h < 24; h++) hourBox.Items.Add(h.ToString("D2"));
            for (int m = 0; m < 60; m += 5) minuteBox.Items.Add(m.ToString("D2"));
            hourBox.SelectedIndex = 9;
            minuteBox.SelectedIndex = 0;

            addButton = new Button
            {
                Text = "➕ Add Event",
                Top = 265,
                Left = 20,
                Width = 110,
                Height = 35,
                FlatStyle = FlatStyle.Flat
            };
            deleteButton = new Button
            {
                Text = "🗑️ Delete Event",
                Top = 265,
                Left = 170,
                Width = 110,
                Height = 35,
                FlatStyle = FlatStyle.Flat
            };

            calendar = new MonthCalendar { Top = 320, Left = 20, MaxSelectionCount = 1 };
            calendar.DateChanged += Calendar_DateChanged;

            eventListLabel = new Label { Text = "📋 Events", Font = new Font("Segoe UI", 12, FontStyle.Bold), Top = 20, Left = 20, AutoSize = true };
            eventList = new ListBox
            {
                Top = 60,
                Left = 20,
                Width = 550,
                Height = 450,
                Font = new Font("Segoe UI", 10),
                BorderStyle = BorderStyle.FixedSingle
            };

            // ----- Events -----
            addButton.Click += addButton_Click;
            deleteButton.Click += deleteButton_Click;
            addEventButton.Click += (s, e) => addButton.PerformClick();
            deleteEventButton.Click += (s, e) => deleteButton.PerformClick();
            toggleThemeButton.Click += (s, e) => { isDark = !isDark; ApplyTheme(); };

            // ----- Layout -----
            leftPanel.Controls.Add(headerLabel);
            leftPanel.Controls.Add(titleLabel);
            leftPanel.Controls.Add(titleBox);
            leftPanel.Controls.Add(dateLabel);
            leftPanel.Controls.Add(datePicker);
            leftPanel.Controls.Add(timeLabel);
            leftPanel.Controls.Add(hourBox);
            leftPanel.Controls.Add(minuteBox);
            leftPanel.Controls.Add(addButton);
            leftPanel.Controls.Add(deleteButton);
            leftPanel.Controls.Add(calendar);

            rightPanel.Controls.Add(eventListLabel);
            rightPanel.Controls.Add(eventList);

            Controls.Add(leftPanel);
            Controls.Add(rightPanel);
        }

        private void ApplyTheme()
        {
            Color bg = isDark ? Color.FromArgb(30, 30, 30) : Color.WhiteSmoke;
            Color fg = isDark ? Color.White : Color.Black;
            Color panel = isDark ? Color.FromArgb(40, 40, 40) : Color.White;
            Color textbox = isDark ? Color.FromArgb(60, 60, 60) : Color.White;

            BackColor = bg;
            leftPanel.BackColor = panel;
            rightPanel.BackColor = panel;

            foreach (Control ctrl in leftPanel.Controls)
            {
                if (ctrl is Label) ctrl.ForeColor = fg;
                if (ctrl is TextBox)
                {
                    ctrl.BackColor = textbox;
                    ctrl.ForeColor = fg;
                }
            }

            foreach (Control ctrl in rightPanel.Controls)
            {
                if (ctrl is Label) ctrl.ForeColor = fg;
            }

            eventList.BackColor = textbox;
            eventList.ForeColor = fg;

            addButton.BackColor = isDark ? Color.MediumSeaGreen : Color.Green;
            addButton.ForeColor = Color.White;
            deleteButton.BackColor = isDark ? Color.IndianRed : Color.Red;
            deleteButton.ForeColor = Color.White;

            toggleThemeButton.Text = isDark ? "🌙 Dark Mode" : "☀️ Light Mode";
            toolStrip.BackColor = isDark ? Color.FromArgb(50, 50, 50) : Color.Gainsboro;
            toolStrip.ForeColor = fg;
        }

        private void Calendar_DateChanged(object sender, DateRangeEventArgs e)
        {
            LoadEventsForDate(e.Start.Date);
        }

        private void LoadEventsForDate(DateTime date)
        {
            eventList.Items.Clear();
            var events = _db.Events
                .Where(ev => ev.ScheduledDate.Date == date)
                .OrderBy(ev => ev.ScheduledDate)
                .ToList();

            foreach (var ev in events)
            {
                eventList.Items.Add($"{ev.ScheduledDate:hh:mm tt} - {ev.Title}");
            }
        }

        private void addButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(titleBox.Text))
            {
                MessageBox.Show("Event title cannot be empty.", "Missing Title", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (hourBox.SelectedIndex == -1 || minuteBox.SelectedIndex == -1)
            {
                MessageBox.Show("Please select a time.", "Missing Time", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int hour = int.Parse(hourBox.SelectedItem.ToString());
            int minute = int.Parse(minuteBox.SelectedItem.ToString());

            var dateTime = new DateTime(
                datePicker.Value.Year,
                datePicker.Value.Month,
                datePicker.Value.Day,
                hour,
                minute,
                0
            );

            var newEvent = new ScheduleEvent
            {
                Title = titleBox.Text.Trim(),
                ScheduledDate = dateTime
            };

            _db.Events.Add(newEvent);
            _db.SaveChanges();

            if (newEvent.ScheduledDate.Date == calendar.SelectionStart.Date)
                LoadEventsForDate(newEvent.ScheduledDate.Date);

            MessageBox.Show("Event added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            titleBox.Clear();
        }


        private void deleteButton_Click(object sender, EventArgs e)
        {
            if (eventList.SelectedIndex >= 0)
            {
                var confirm = MessageBox.Show("Are you sure you want to delete the selected event?", "Confirm Deletion", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (confirm == DialogResult.No) return;

                var selectedDate = calendar.SelectionStart.Date;
                var events = _db.Events
                    .Where(ev => ev.ScheduledDate.Date == selectedDate)
                    .OrderBy(ev => ev.ScheduledDate)
                    .ToList();

                _db.Events.Remove(events[eventList.SelectedIndex]);
                _db.SaveChanges();
                LoadEventsForDate(selectedDate);

                MessageBox.Show("Event deleted successfully.", "Deleted", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Please select an event to delete.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

    }
}
