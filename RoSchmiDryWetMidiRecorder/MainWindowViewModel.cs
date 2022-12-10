using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Multimedia;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Media;

namespace RoSchmiDryWetMidiUtility
{
    internal class MainWindowViewModel : ObservableObject
    {
        private string button1Text = "Start Recording";
        private string button2Text = " Not used ";
        private string typeTextboxText = "";
        private ICollection<Melanchall.DryWetMidi.Multimedia.InputDevice>? midiDevices;
        private ObservableCollection<string> midiDevicesNames = new ObservableCollection<string>();
        private string? selectedMidiDeviceName;
        private int currentProgress;
        private Visibility progressVisibility = Visibility.Hidden;

        private string selectedRootPath = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);

        private List<string> rootPaths = new List<string>(new string[] { Environment.GetFolderPath(Environment.SpecialFolder.MyMusic) ,
                                                              Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) });

        private string subFolder = "DryWetMidiFiles";
        private string fileName = "RecordedMidiFile";

        
        private bool rootPathComboIsEnabled = true;
        private bool pathIsFixed = false;


        private string? actNote;
        private Brush noteTextColor = Brushes.Black;
        
        private Recording? recording;

        public IRelayCommand Button1Command { get; }

        public IRelayCommand RefreshButtonCommand { get; }

        public IRelayCommand Button2Command { get; }



        private IInputDevice? inputDevice;

        #region Constructor
        public MainWindowViewModel()

        {
            Button1Command = new RelayCommand(DoButton1Command);
            Button2Command = new RelayCommand(DoButton2Command);
            RefreshButtonCommand = new RelayCommand(DoRefreshButtonCommand);

            DoRefreshButtonCommand(); 
            ProgressVisibility = Visibility.Hidden;
        }
        #endregion

        public string Button1Text { get => button1Text; set { _ = SetProperty(ref button1Text, value); } }
        public string Button2Text { get => button2Text; set { _ = SetProperty(ref button2Text, value); } }

        public string TypeTextboxText { get => typeTextboxText; set { _ = SetProperty(ref typeTextboxText, value); } }

        public ICollection<Melanchall.DryWetMidi.Multimedia.InputDevice> MidiDevices { get => midiDevices; set { _ = SetProperty(ref midiDevices, value); } }
        public ObservableCollection<string> MidiDevicesNames { get => midiDevicesNames; set { _ = SetProperty(ref midiDevicesNames, value); } }

        public string SelectedMidiDeviceName { get => selectedMidiDeviceName; set { _ = SetProperty(ref selectedMidiDeviceName, value); } }

        public string? ActNote { get => actNote; set { _ = SetProperty(ref actNote, value); } }

        public Brush NoteTextColor { get => noteTextColor; set { _ = SetProperty(ref noteTextColor, value); } }

        public int CurrentProgress { get => currentProgress; set { _ = SetProperty(ref currentProgress, value); } }

        public Visibility ProgressVisibility { get => progressVisibility; set { _ = SetProperty(ref progressVisibility, value); } }


        public List<string>RootPaths { get => rootPaths; set { _ = SetProperty(ref rootPaths, value); } }

        public string SelectedRootPath { get => selectedRootPath; set { _ = SetProperty(ref selectedRootPath, value); } }

        public string SubFolder { get => subFolder; set { _ = SetProperty(ref subFolder, value); } }
        public string FileName { get => fileName; set { _ = SetProperty(ref fileName, value); } }

        public bool PathIsFixed { get => pathIsFixed; set { _ = SetProperty(ref pathIsFixed, value); } }

        public bool RootPathComboIsEnabled { get => rootPathComboIsEnabled; set { _ = SetProperty(ref rootPathComboIsEnabled, value); } }


        #region method DoButton1Comman
        private void DoButton1Command()
        {
            string combPath = Path.Combine(SelectedRootPath, SubFolder);

            if (Button1Text == "Start Recording")
            {
                bool preConditionsMet = true;
                try
                {
                    if (!Directory.Exists(combPath))
                    {
                        Directory.CreateDirectory(combPath);
                    }
                }
                catch
                {
                    preConditionsMet = false;
                    MessageBox.Show("RootPath and Subfolder do not represent a valid path");
                }

                if (  ! (fileName.IndexOfAny(  Path.GetInvalidFileNameChars() ) == -1)   )
                {
                    MessageBox.Show("FilenName contains invalid characters");
                    preConditionsMet = false;
                }
              
                if (preConditionsMet)
                {
                    Button1Text = "Stop Recording";
                    recording = new Recording(TempoMap.Default, inputDevice);
                    recording.Start();
                    CurrentProgress = 0;
                    RootPathComboIsEnabled = false;
                    PathIsFixed = true;                 
                    ProgressVisibility = Visibility.Visible;
                }
            }
            else 
            {
                Button1Text = "Start Recording";
                
                if (recording != null)
                {
                    recording.Stop();

                    var recordedFile = recording.ToFile();
                    recording.Dispose();

                    string recordedFileName = "";

                    if (File.Exists(Path.Combine(combPath, FileName + ".mid")))
                    {
                        string searchstring = Path.Combine(combPath, FileName + " (*).mid");
                        string[] matchFiles = Directory.GetFiles(combPath, FileName + " (*).mid");
                        for  (int i = 0; i < matchFiles.Length; i++)
                        {
                            matchFiles[i] = matchFiles[i].Substring(0, matchFiles[i].LastIndexOf(')'));
                            matchFiles[i] = matchFiles[i].Substring(matchFiles[i].LastIndexOf('(') + 1);                           
                        }
                        int highIdx = 0;
                        int runIdx = 0;
                        for (int i = 0; i < matchFiles.Length; i++)
                        {
                            if (int.TryParse(matchFiles[i], out runIdx))
                            {
                                highIdx = runIdx > highIdx ? runIdx : highIdx;
                            }
                        }
                        recordedFileName = Path.Combine(combPath, FileName + " (" + (highIdx + 1).ToString() + ").mid");
                        
                        recordedFile.Write(recordedFileName,overwriteFile: false);
                                              
                    }
                    else
                    {
                        recordedFile.Write(Path.Combine(combPath, FileName + ".mid"));
                    }
                    
                    PathIsFixed = false;
                    RootPathComboIsEnabled = true;
                    
                    ProgressVisibility = Visibility.Hidden;
                    MessageBox.Show("Record was written to:\n" + recordedFileName);

                }
            }
        }
        #endregion

        #region method DoRefreshButtonComma
        private void DoRefreshButtonCommand()
        {
            MidiDevicesNames.Clear();
            MidiDevices = Melanchall.DryWetMidi.Multimedia.InputDevice.GetAll();
            
            if (midiDevices != null && MidiDevices.Count > 0)
            {
                foreach (var midiDevice in MidiDevices)
                {
                    MidiDevicesNames.Add(midiDevice.Name);
                }
                if (!MidiDevicesNames.Contains(SelectedMidiDeviceName))
                {
                    SelectedMidiDeviceName = midiDevicesNames[0];
                }

                (inputDevice as IDisposable)?.Dispose();

                inputDevice = Melanchall.DryWetMidi.Multimedia.InputDevice.GetByName(SelectedMidiDeviceName);
                inputDevice.EventReceived += OnEventReceived;
                if (!inputDevice.IsListeningForEvents)
                {
                    inputDevice.StartEventsListening();
                }
            }
        }
        #endregion

        private void DoButton2Command()
        {
            Button2Text = "Clicked_2";
        }

        #region eventhandler  OnEventReceived
        private void OnEventReceived(object? sender, MidiEventReceivedEventArgs e)
        {
            CurrentProgress = ++CurrentProgress % 20;
            if (e.Event.EventType == MidiEventType.NoteOn)
            {
                var noteOnEvent = (NoteOnEvent)e.Event;
                var theNote =  noteOnEvent.GetNoteName();
                this.ActNote = theNote.ToString();
                this.NoteTextColor = Brushes.Black; 
                this.TypeTextboxText = e.Event.EventType.ToString();
            }
            else
            {
                if((e.Event.EventType == MidiEventType.NoteOff))
                {
                    var noteOffEvent = (NoteOffEvent)e.Event;
                    var theNote = noteOffEvent.GetNoteName();
                    this.ActNote = theNote.ToString();
                    this.NoteTextColor = Brushes.LightGray;
                    this.TypeTextboxText = e.Event.EventType.ToString();
                }
            }
        }
        #endregion
    }
}
