using Android.Bluetooth;

using Java.IO;
using Java.Util;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Console = System.Console;

namespace RfidStationControl
{
    public class BtConnector
    {
        private CancellationTokenSource _ct { get; set; }

        public int ConnectionTimeout = 5000;

        public volatile List<byte> BtInputBuffer = new List<byte>();

        public volatile object SerialReceiveThreadLock = new object();

        private bool _isEnabled = false;
        public bool IsBtEnabled()
        {
            return _isEnabled;
        }

        private volatile bool _isConnected = false;
        public bool IsBtConnected()
        {
            return _isConnected;
        }

        private string _deviceName = "";
        public string ConnnectedDevice()
        {
            return _deviceName;
        }

        private volatile string _deviceAddress;
        public string DeviceAddress()
        {
            return _deviceAddress;
        }

        public volatile bool _connecting = false;
        public bool IsBtConnecting()
        {
            return _connecting;
        }

        private BluetoothAdapter BtAdapter = BluetoothAdapter.DefaultAdapter;
        private BluetoothDevice BtDevice;
        private BluetoothSocket BtSocket;
        private InputStreamReader BtStreamReader;
        private BufferedReader BtBufferReader;
        private OutputStreamWriter BtStreamWriter;
        private BufferedWriter BtBufferWriter;

        public async Task<bool> Enable()
        {
            _isEnabled = false;
            BluetoothAdapter BtAdapter = BluetoothAdapter.DefaultAdapter;
            if (BtAdapter != null)
            {
                if (!BtAdapter.IsEnabled)
                {
                    var _manager = (BluetoothManager)Android.App.Application.Context.GetSystemService(Android.Content.Context.BluetoothService);
                    _manager.Adapter.Enable();

                    DateTime enableStarted = DateTime.Now;
                    while (!GlobalOperationsIdClass.Bt.IsBtEnabled() && DateTime.Now.Subtract(enableStarted).TotalMilliseconds < ConnectionTimeout) await Task.Delay(1);

                    BtAdapter = BluetoothAdapter.DefaultAdapter;
                }
                _isEnabled = BtAdapter.IsEnabled;
            }

            return _isEnabled;
        }

        /// <summary>
        /// Connect the "reading" loop 
        /// </summary>
        /// <param name="name">Name of the paired bluetooth device (also a part of the name)</param>
        public async void Connect(string name, int sleepTime = 100)
        {
            if (_isEnabled) await Task.Run(async () => await loop(name, sleepTime));
        }

        private async Task loop(string deviceName, int sleepTime)
        {
            _connecting = true;
            _ct = new CancellationTokenSource();
            DateTime connectionStarted = DateTime.Now;
            try
            {
                while (!_ct.IsCancellationRequested && DateTime.Now.Subtract(connectionStarted).TotalMilliseconds < ConnectionTimeout)
                {
                    Thread.Sleep(sleepTime);
                    System.Diagnostics.Debug.WriteLine("Try to connect to " + deviceName);

                    BtDevice = (from bd in BtAdapter.BondedDevices
                                where bd.Name == deviceName
                                select bd).FirstOrDefault();

                    if (BtDevice == null)
                    {
                        System.Diagnostics.Debug.WriteLine("Device not found: " + BtDevice?.Name);
                        throw new Exception("Device not found: " + deviceName);
                    }

                    UUID uuid = UUID.FromString("00001101-0000-1000-8000-00805f9b34fb");
                    if ((int)Android.OS.Build.VERSION.SdkInt >= 10) // Gingerbread 2.3.3 2.3.4
                        BtSocket = BtDevice.CreateInsecureRfcommSocketToServiceRecord(uuid);
                    else
                        BtSocket = BtDevice.CreateRfcommSocketToServiceRecord(uuid);

                    if (BtSocket != null)
                    {
                        await BtSocket.ConnectAsync();

                        if (BtSocket.IsConnected)
                        {
                            _connecting = false;
                            _isConnected = true;
                            _deviceName = deviceName;
                            _deviceAddress = BtDevice.Address;
                            System.Diagnostics.Debug.WriteLine("Connected!");
                            BtStreamReader = new InputStreamReader(BtSocket.InputStream);
                            BtBufferReader = new BufferedReader(BtStreamReader);
                            BtStreamWriter = new OutputStreamWriter(BtSocket.OutputStream);
                            BtBufferWriter = new BufferedWriter(BtStreamWriter);

                            while (!_ct.IsCancellationRequested)
                            {
                                if (BtBufferReader.Ready())
                                {
                                    byte[] inputBuffer = new byte[1024];

                                    int c = await BtSocket.InputStream.ReadAsync(inputBuffer, 0,
                                        inputBuffer.Length);
                                    lock (SerialReceiveThreadLock)
                                    {
                                        for (int i = 0; i < c; i++) BtInputBuffer.Add(inputBuffer[i]);
                                    }
                                }

                                // A little stop to the uneverending thread...
                                Thread.Sleep(sleepTime);
                                if (!BtSocket.IsConnected)
                                {
                                    System.Diagnostics.Debug.WriteLine(
                                        "BthSocket.IsConnected = false, Throw exception");
                                    throw new Exception("Device disconnected: " + deviceName);
                                }
                            }
                        }
                    }
                    else System.Diagnostics.Debug.WriteLine("BthSocket = null");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("EXCEPTION: " + ex.Message);
            }

            finally
            {
                //if (BtSocket != null) BtSocket.Close();
                //BtDevice = null;
                //BtAdapter = null;

                BtBufferWriter?.Close();
                BtBufferWriter = null;
                BtBufferReader?.Close();
                BtBufferReader = null;
                BtStreamWriter?.Close();
                BtStreamWriter = null;
                BtStreamReader?.Close();
                BtStreamReader = null;
                BtSocket?.Close();
                BtSocket = null;
                BtDevice = null;
                BtAdapter = BluetoothAdapter.DefaultAdapter;
                System.Diagnostics.Debug.WriteLine("Exit the connection loop");
                _connecting = false;
                _isConnected = false;
                _deviceName = "";
                _deviceAddress = "";
            }
        }

        /// <summary>
        /// Close the Reading loop
        /// </summary>
        /// <returns><c>true</c> if this instance cancel ; otherwise, <c>false</c>.</returns>
        public void Close()
        {
            if (_ct != null)
            {
                _ct.Cancel();
            }
            _connecting = false;
            _isConnected = false;
            _deviceName = "";
            _deviceAddress = "";
        }

        public async Task<bool> WriteBtAsync(byte[] Data)
        {
            try
            {
                await BtSocket.OutputStream.WriteAsync(Data, 0, Data.Length);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                //throw;
                Close();
                return false;
            }

            return true;
        }

        public ObservableCollection<string> PairedDevices()
        {
            BtAdapter = BluetoothAdapter.DefaultAdapter;
            ObservableCollection<string> devices = new ObservableCollection<string>();

            foreach (var bd in BtAdapter.BondedDevices)
                devices.Add(bd.Name);

            return devices;
        }
    }
}