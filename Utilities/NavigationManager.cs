using System;
using System.Windows.Controls;

namespace AhorcadoClient.Utilities
{
    public class NavigationManager
    {
        private static NavigationManager _instance;
        private Frame _mainFrame;

        public static NavigationManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    throw new InvalidOperationException("NavigationManager no ha sido inicializado.");
                }
                return _instance;
            }
        }

        private NavigationManager(Frame frame)
        {
            _mainFrame = frame;
        }

        public static void Initialize(Frame frame)
        {
            if (_instance == null)
            {
                _instance = new NavigationManager(frame);
            }
        }

        public static void Reset()
        {
            _instance = null;
        }

        public void NavigateToPage(Page pageInstance)
        {
            _mainFrame.Navigate(pageInstance);
        }

        public void ClearNavigation()
        {
            _mainFrame.Content = null;
        }
        public Page CurrentPage => _mainFrame?.Content as Page;

        public T CurrentPageAs<T>() where T : Page
        {
            return _mainFrame?.Content as T;
        }
    }
}