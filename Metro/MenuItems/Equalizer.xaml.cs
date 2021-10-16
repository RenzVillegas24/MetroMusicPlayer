using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using BandedSpectrumAnalyzer;
using Un4seen.Bass;

namespace Metro.MenuItems
{
    /// <summary>
    /// Interaction logic for Equalizer.xaml
    /// </summary>
    public partial class Equalizer : UserControl
    {
        public Equalizer()
        {
            InitializeComponent();
        }
        private int[] _fxEQ = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

        private void Call_Eq()
        {

            BASS_DX8_PARAMEQ eq = new BASS_DX8_PARAMEQ();
            _fxEQ[0] = Bass.BASS_ChannelSetFX(BassEngine.Instance.ActiveStreamHandle, BASSFXType.BASS_FX_DX8_PARAMEQ, 0);
            _fxEQ[1] = Bass.BASS_ChannelSetFX(BassEngine.Instance.ActiveStreamHandle, BASSFXType.BASS_FX_DX8_PARAMEQ, 0);
            _fxEQ[2] = Bass.BASS_ChannelSetFX(BassEngine.Instance.ActiveStreamHandle, BASSFXType.BASS_FX_DX8_PARAMEQ, 0);
            _fxEQ[3] = Bass.BASS_ChannelSetFX(BassEngine.Instance.ActiveStreamHandle, BASSFXType.BASS_FX_DX8_PARAMEQ, 0);
            _fxEQ[4] = Bass.BASS_ChannelSetFX(BassEngine.Instance.ActiveStreamHandle, BASSFXType.BASS_FX_DX8_PARAMEQ, 0);
            _fxEQ[5] = Bass.BASS_ChannelSetFX(BassEngine.Instance.ActiveStreamHandle, BASSFXType.BASS_FX_DX8_PARAMEQ, 0);
            _fxEQ[6] = Bass.BASS_ChannelSetFX(BassEngine.Instance.ActiveStreamHandle, BASSFXType.BASS_FX_DX8_PARAMEQ, 0);
            _fxEQ[7] = Bass.BASS_ChannelSetFX(BassEngine.Instance.ActiveStreamHandle, BASSFXType.BASS_FX_DX8_PARAMEQ, 0);
            _fxEQ[8] = Bass.BASS_ChannelSetFX(BassEngine.Instance.ActiveStreamHandle, BASSFXType.BASS_FX_DX8_PARAMEQ, 0);
            _fxEQ[9] = Bass.BASS_ChannelSetFX(BassEngine.Instance.ActiveStreamHandle, BASSFXType.BASS_FX_DX8_PARAMEQ, 0);

            eq.fBandwidth = 36f;
            
            eq.fCenter = 80f;
            eq.fGain = (float)slider1.Value;
            label1.Content = eq.fGain;
            Bass.BASS_FXSetParameters(_fxEQ[0], eq);

            eq.fCenter = 160f;
            eq.fGain = (float)slider2.Value;
            label2.Content = eq.fGain;
            Bass.BASS_FXSetParameters(_fxEQ[1], eq);

            eq.fCenter = 300f;
            eq.fGain = (float)slider3.Value;
            label3.Content = eq.fGain;
            Bass.BASS_FXSetParameters(_fxEQ[2], eq);

            eq.fCenter = 500f;
            eq.fGain = (float)slider4.Value;
            label4.Content = eq.fGain;
            Bass.BASS_FXSetParameters(_fxEQ[3], eq);

            eq.fCenter = 1000f;
            eq.fGain = (float)slider5.Value;
            label5.Content = eq.fGain;
            Bass.BASS_FXSetParameters(_fxEQ[4], eq);

            eq.fCenter = 3000f;
            eq.fGain = (float)slider6.Value;
            label6.Content = eq.fGain;
            Bass.BASS_FXSetParameters(_fxEQ[5], eq);

            eq.fCenter = 6000f;
            eq.fGain = (float)slider7.Value;
            label7.Content = eq.fGain;
            Bass.BASS_FXSetParameters(_fxEQ[6], eq);

            eq.fCenter = 12000f;
            eq.fGain = (float)slider8.Value;
            label8.Content = eq.fGain;
            Bass.BASS_FXSetParameters(_fxEQ[7], eq);

            eq.fCenter = 14000f;
            eq.fGain = (float)slider9.Value;
            label9.Content = eq.fGain;
            Bass.BASS_FXSetParameters(_fxEQ[8], eq);

            eq.fCenter = 16000f;
            eq.fGain = (float)slider10.Value;
            label10.Content = eq.fGain;
            Bass.BASS_FXSetParameters(_fxEQ[9], eq);

        }

        private void  GainChange_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Call_Eq();
        }

    }
}
