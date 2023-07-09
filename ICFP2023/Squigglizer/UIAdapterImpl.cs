using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ICFP2023
{
    internal class UIAdapterImpl : UIAdapter
    {
        private readonly MainWindow _ui;
        private readonly CancellationToken _cancellationToken;
        private long _rendering = 0;
        private Task _renderPump;
        private Solution _nextSolution;

        public UIAdapterImpl(MainWindow ui, CancellationToken cancellationToken)
        {
            _ui = ui;
            _cancellationToken = cancellationToken;

            _renderPump = Task.Run(RenderLoop);
        }


        private void RenderLoop()
        {
            // This is wrong but I just want to render *something*
            while (!_cancellationToken.IsCancellationRequested)
            {
                if (Interlocked.Exchange(ref _rendering, 1) == 0)
                {
                    // We can do stuff
                    _ui.Dispatcher.BeginInvoke(() =>
                    {
                        Solution toRender = Interlocked.Exchange(ref _nextSolution, null);

                        if (toRender != null)
                        {
                            _ui.RenderSolution(toRender);
                        }

                        Interlocked.Exchange(ref _rendering, 0);
                    });
                }

                Task.Delay(20).Wait(); // Need something to stop us from constantly spamming the UI
            }
        }

        public void Render(Solution solution)
        {
            Interlocked.Exchange(ref _nextSolution, solution);
        }

        public bool ShouldHalt()
        {
            return _cancellationToken.IsCancellationRequested;
        }

        public void SetMusicianColor(int index, string color)
        {
            // Lazy hack to make sure things have rendered
            Task.Delay(20).Wait();
            _ui.Dispatcher.BeginInvoke(() =>
            {
                _ui.SetMusicianColor(index, color);
            });
        }

        public void SetAllMusicianColors(string[] colors)
        {
            // Lazy hack to make sure things have rendered
            Task.Delay(20).Wait();
            _ui.Dispatcher.BeginInvoke(() =>
            {
                for (int i = 0; i < colors.Length; i++)
                {
                    _ui.SetMusicianColor(i, colors[i]);
                }
            });
        }

        public void ClearAllColors()
        {
            // Lazy hack to make sure things have rendered
            Task.Delay(20).Wait();
            _ui.Dispatcher.BeginInvoke(() =>
            {
                _ui.ClearAllColors();
            });
        }

        public void ClearMusicianColor(int index)
        {
            // Lazy hack to make sure things have rendered
            Task.Delay(20).Wait();
            _ui.Dispatcher.BeginInvoke(() =>
            {
                _ui.ClearMusicianColor(index);
            });
        }
    }
}
