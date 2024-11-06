﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ClassIsland.Core.Abstractions.Controls;
using ClassIsland.Core.Attributes;
using MahApps.Metro.Controls;
using MaterialDesignThemes.Wpf;

namespace ExtraIsland.Components;

[ComponentInfo(
    "0EA67B3B-E4CB-56C1-AFDC-F3EA7F38924D",
    "流畅时钟",
    PackIconKind.ClockDigital,
    "拥有动画支持"
)]
public partial class FluentClock : ComponentBase<FluentClockConfig> {
    public FluentClock() {
        InitializeComponent();
    }

    void LoadCache() {
        try {
            _tripleEaseCache.Add(0,0.0);
            for (int x = 1; x <= 40; x++) {
                _tripleEaseCache.Add(x,40 * TripleEase(x / 40.0,1));
            }
            for (int x = 1; x <= 40; x++) {
                _tripleEaseCache.Add(-x,-40 * TripleEase(1 - x / 40.0,1));
            }
        }
        catch {
            // ignored
        }
    }
    
    void DetectCycle() {
        LoadCache();
        string hours = string.Empty;
        string minutes = string.Empty;
        string seconds = string.Empty;
        bool sparkSeq = true;
        Settings.IsAccurate ??= true;
        while (true) {
            if (Settings.IsAccurate.Value) {
                this.Invoke(() => {
                    LSecs.Visibility = Visibility.Visible;
                    SSecs.Visibility = Visibility.Visible;
                });
            } else {
                this.Invoke(() => {
                    LSecs.Visibility = Visibility.Collapsed;
                    SSecs.Visibility = Visibility.Collapsed;
                });
            }
            var now = DateTime.Now;
            if (hours != now.Hour.ToString()) {
                hours = now.Hour.ToString();
                var h = hours;
                SwapAnim(LHours,HTt,h);
            }
            if (minutes != now.Minute.ToString()) {
                minutes = now.Minute.ToString();
                var m = minutes;
                if (m.Length == 1) {
                    m = "0" + m;
                }
                SwapAnim(LMins,MTt,m);
            }
            if (seconds != now.Second.ToString()) {
                seconds = now.Second.ToString();
                if (Settings.IsAccurate.Value) {
                    this.Invoke(() => {
                        SMins.Opacity = 1;
                    });
                    var s = seconds;
                    if (s.Length == 1) {
                        s = "0" + s;
                    }
                    SwapAnim(LSecs,STt,s);
                } else {
                    if (sparkSeq) {
                        for (int  x = 0;  x <= 40;  x++) {
                            int x1 = x;
                            this.Invoke(() => {
                                SMins.Opacity = (40 - x1) / 40.0;
                            });
                            Thread.Sleep(1);
                        }
                        sparkSeq = false;
                    } else {
                        for (int x = 0; x <= 40; x++) {
                            int x1 = x;
                            this.Invoke(() => {
                                SMins.Opacity = 1 - (40 - x1) / 40.0;
                            });
                            Thread.Sleep(1);
                        }
                        sparkSeq = true;
                    }
                }
            }
            Thread.Sleep(60);
        }
        // ReSharper disable once FunctionNeverReturns
    }

    readonly Dictionary<int,double> _tripleEaseCache = new Dictionary<int,double>();
    
    double TripleEase(double tick,double scale = 1,double multiplier = 1) {
        return multiplier * (double.Pow(tick * scale,3));
    }
    
    void SwapAnim(Label target,TranslateTransform targetTransform, string newContent) {
        for (int  x = 0;  x <= 40;  x++) {
            int x1 = x;
            this.Invoke(() => {
                targetTransform.Y = _tripleEaseCache[x1];
                target.Opacity = (40 - x1) / 40.0;
            });
            //AccurateWait(0.1);
            Thread.Sleep(1);
        }
        this.Invoke(() => {
            target.Content = newContent;
        });
        for (int  x = 0;  x <= 40;  x++) {
            int x1 = x;
            this.Invoke(() => {
                targetTransform.Y = _tripleEaseCache[-x1];
                target.Opacity =1 - (40 - x1) / 40.0;
            });
            Thread.Sleep(1);
        }
    }
    
    void FluentClock_OnLoaded(object sender,RoutedEventArgs e) {
        new Thread(DetectCycle).Start();
    }
}