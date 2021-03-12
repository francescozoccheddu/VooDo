
using System.ComponentModel;

namespace VooDo.Testbench.Desktop
{
    public sealed partial class MainWindow
    {
        private sealed class Status : INotifyPropertyChanged
        {
            private bool m_isPaneOpen;
            private bool m_isFrameButtonOpen;
            private bool m_isSnakeButtonOpen;
            private bool m_isHeaderExpanded;
            private Item m_selectedItem = Item.CreateItem();

            public Item SelectedItem
            {
                get => m_selectedItem;
                set
                {
                    if (value != m_selectedItem)
                    {
                        m_selectedItem = value;
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedItem)));
                        Update();
                    }
                }
            }

            public bool IsHeaderExpanded
            {
                get => m_isHeaderExpanded;
                set
                {
                    if (value != m_isHeaderExpanded)
                    {
                        m_isHeaderExpanded = value;
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsHeaderExpanded)));
                        Update();
                    }
                }
            }

            public bool IsSnakeButtonOpen
            {
                get => m_isSnakeButtonOpen;
                set
                {
                    if (value != m_isSnakeButtonOpen)
                    {
                        m_isSnakeButtonOpen = value;
                        PropertyChanged?.Invoke(this, new(nameof(IsSnakeButtonOpen)));
                        Update();
                    }
                }
            }

            public bool IsFrameButtonOpen
            {
                get => m_isFrameButtonOpen;
                set
                {
                    if (value != m_isFrameButtonOpen)
                    {
                        m_isFrameButtonOpen = value;
                        PropertyChanged?.Invoke(this, new(nameof(IsFrameButtonOpen)));
                        Update();
                    }
                }
            }

            public bool IsPaneOpen
            {
                get => m_isPaneOpen;
                set
                {
                    if (value != m_isPaneOpen)
                    {
                        m_isPaneOpen = value;
                        PropertyChanged?.Invoke(this, new(nameof(IsPaneOpen)));
                        Update();
                    }
                }
            }

            private void Update()
            {
                bool wasSomethingFocused = IsSomethingFocused;
                IsSomethingFocused = m_isPaneOpen || m_isHeaderExpanded || m_isSnakeButtonOpen || m_isFrameButtonOpen;
                if (wasSomethingFocused != IsSomethingFocused)
                {
                    PropertyChanged?.Invoke(this, new(nameof(IsSomethingFocused)));
                }
            }

            public bool IsSomethingFocused
            {
                get; private set;
            }

            public event PropertyChangedEventHandler? PropertyChanged;
        }

    }
}
