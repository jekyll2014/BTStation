﻿using Android.Bluetooth;

using Java.IO;
using Java.Util;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RfidStationControl
{
    public class BtConnector
    {
        private CancellationTokenSource CancellationToken { get; set; }

        private const int CONNECTION_TIMEOUT = 5000;

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

        public volatile bool Connecting = false;

        public bool IsBtConnecting()
        {
            return Connecting;
        }

        private BluetoothAdapter _btAdapter = BluetoothAdapter.DefaultAdapter;
        private BluetoothDevice _btDevice;
        private BluetoothSocket _btSocket;
        private InputStreamReader _btStreamReader;
        private BufferedReader _btBufferReader;
        private OutputStreamWriter _btStreamWriter;
        private BufferedWriter _btBufferWriter;

        public async Task<bool> Enable()
        {
            _isEnabled = false;
            var BtAdapter = BluetoothAdapter.DefaultAdapter;
            if (BtAdapter == null) return _isEnabled;

            if (!BtAdapter.IsEnabled)
            {
                var _manager = (BluetoothManager)Android.App.Application.Context.GetSystemService(Android.Content.Context.BluetoothService);
                _manager.Adapter.Enable();

                var enableStarted = DateTime.Now;
                while (!GlobalOperationsIdClass.Bt.IsBtEnabled() && DateTime.Now.Subtract(enableStarted).TotalMilliseconds < CONNECTION_TIMEOUT) await Task.Delay(1);

                BtAdapter = BluetoothAdapter.DefaultAdapter;
            }
            _isEnabled = BtAdapter.IsEnabled;

            return _isEnabled;
        }

        /// <summary>
        /// Connect the "reading" loop 
        /// </summary>
        /// <param name="name">Name of the paired bluetooth device (also a part of the name)</param>
        /// <param name="sleepTime"></param>
        public async void Connect(string name, int sleepTime = 100)
        {
            if (_isEnabled) await Task.Run(async () => await Loop(name, sleepTime));
        }

        private async Task Loop(string deviceName, int sleepTime)
        {
            Connecting = true;
            CancellationToken = new CancellationTokenSource();
            var connectionStarted = DateTime.Now;
            try
            {
                while (!CancellationToken.IsCancellationRequested && DateTime.Now.Subtract(connectionStarted).TotalMilliseconds < CONNECTION_TIMEOUT)
                {
                    Thread.Sleep(sleepTime);
                    System.Diagnostics.Debug.WriteLine("Try to connect to " + deviceName);

                    _btDevice = (from bd in _btAdapter.BondedDevices
                                where bd.Name == deviceName
                                select bd).FirstOrDefault();

                    if (_btDevice == null)
                    {
                        System.Diagnostics.Debug.WriteLine("Device not found: " + _btDevice?.Name);
                        throw new Exception("Device not found: " + deviceName);
                    }

                    var uuid = UUID.FromString("00001101-0000-1000-8000-00805f9b34fb");
                    if ((int)Android.OS.Build.VERSION.SdkInt >= 10) // Gingerbread 2.3.3 2.3.4
                        _btSocket = _btDevice.CreateInsecureRfcommSocketToServiceRecord(uuid);
                    else
                        _btSocket = _btDevice.CreateRfcommSocketToServiceRecord(uuid);

                    if (_btSocket != null)
                    {
                        await _btSocket.ConnectAsync();

                        if (!_btSocket.IsConnected) continue;

                        Connecting = false;
                        _isConnected = true;
                        _deviceName = deviceName;
                        _deviceAddress = _btDevice.Address;
                        System.Diagnostics.Debug.WriteLine("Connected!");
                        _btStreamReader = new InputStreamReader(_btSocket.InputStream);
                        _btBufferReader = new BufferedReader(_btStreamReader);
                        _btStreamWriter = new OutputStreamWriter(_btSocket.OutputStream);
                        _btBufferWriter = new BufferedWriter(_btStreamWriter);

                        while (!CancellationToken.IsCancellationRequested)
                        {
                            if (_btBufferReader.Ready())
                            {
                                var inputBuffer = new byte[1024];

                                var c = await _btSocket.InputStream.ReadAsync(inputBuffer, 0,
                                    inputBuffer.Length);
                                lock (SerialReceiveThreadLock)
                                {
                                    for (var i = 0; i < c; i++) BtInputBuffer.Add(inputBuffer[i]);
                                }
                            }

                            // A little stop to the uneverending thread...
                            Thread.Sleep(sleepTime);
                            if (_btSocket.IsConnected) continue;

                            System.Diagnostics.Debug.WriteLine(
                                "BthSocket.IsConnected = false, Throw exception");
                            throw new Exception("Device disconnected: " + deviceName);
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("BthSocket = null");
                    }
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

                _btBufferWriter?.Close();
                _btBufferWriter = null;
                _btBufferReader?.Close();
                _btBufferReader = null;
                _btStreamWriter?.Close();
                _btStreamWriter = null;
                _btStreamReader?.Close();
                _btStreamReader = null;
                _btSocket?.Close();
                _btSocket = null;
                _btDevice = null;
                _btAdapter = BluetoothAdapter.DefaultAdapter;
                System.Diagnostics.Debug.WriteLine("Exit the connection loop");
                Connecting = false;
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
            CancellationToken?.Cancel();
            Connecting = false;
            _isConnected = false;
            _deviceName = "";
            _deviceAddress = "";
        }

        public async Task<bool> WriteBtAsync(byte[] Data)
        {
            try
            {
                await _btSocket.OutputStream.WriteAsync(Data, 0, Data.Length);
            }
            catch (Exception)
            {
                //Console.WriteLine(e.Message);
                Close();
                return false;
            }

            return true;
        }

        public ObservableCollection<string> PairedDevices()
        {
            _btAdapter = BluetoothAdapter.DefaultAdapter;
            var devices = new ObservableCollection<string>();

            foreach (var bd in _btAdapter.BondedDevices)
                devices.Add(bd.Name);

            return devices;
        }
    }
}
