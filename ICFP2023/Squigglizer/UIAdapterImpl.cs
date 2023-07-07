﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ICFP2023
{
    internal class UIAdapterImpl : UIAdapter
    {
        private readonly MainWindow _ui;

        private long _rendering = 0;
        private Task _renderPump;
        private Solution _nextSolution;

        public UIAdapterImpl(MainWindow ui)
        {
            _ui = ui;
            _renderPump = Task.Run(RenderLoop);
        }


        private void RenderLoop()
        {
            // This is wrong but I just want to render *something*
            while (true)
            {
                if (Interlocked.Exchange(ref _rendering, 1) == 0)
                {
                    // We can do stuff
                    _ui.Dispatcher.BeginInvoke(() =>
                    {
                        Solution toRender;
                        int score;
                        int totalCost;
                        toRender = Interlocked.Exchange(ref _nextSolution, null);

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
            return false;
        }
    }
}