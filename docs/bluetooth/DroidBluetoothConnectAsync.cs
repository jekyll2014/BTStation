/*
  Source:
  https://forums.xamarin.com/discussion/30055/connecting-to-a-bluetooth-scanner-with-xamarin-android#Comment_94845
  
  The problem relates to device.GetUuids. You are assuming the Uuid is what you
  want with ElementAt(0). What you need is a loop inside your foreach, like in
  the following testing, that the device supports the Bluetooth Serial Port Profile
*/

  ParcelUuid[] uuids = device.GetUuids();
  for (int j = 0; j < uuids.Length; j++)
  {
    if (uuids[j].Uuid.ToString() == BluetoothService.SerialPort.ToString())
    {
    // We wont this one
    entries[i] = device.Name;
    entryValues[i] = device.Address;
    i++;
    }
  }

/*
  The above code is the type of code you would use in a preference screen, where you
  want to limit the devices shown to a user that are Bluetooth Serial Port Profile
  devices only, rather than all Bluetooth devices. This works on all versions above
  4.0.3. If you need it for 4.0.3 and lower versions (I've not tested below 4.0.3),
  then you will need to display all Bluetooth devices, rather than just SPP devices.
  
  All you really need for the connection part, if you know the device is a SPP device
  is the following. Presuming you have already stored the bluetooth address of the
  device in your preferences.
*/
public async Task ConnectAsync()
{
  // Get the remote device, the bluetooth address has already been verified
  bluetoothDevice = bluetoothAdapter.GetRemoteDevice(bluetoothAddress);
  try
  {
    bluetoothSocket = bluetoothDevice.CreateRfcommSocketToServiceRecord(BluetoothService.SerialPort);
  }
  catch (Java.IO.IOException e)
  {
    Log.Error(TAG, "Create Socket Failed", e);
  }

  connected = false;
  if (bluetoothSocket != null)
  {
    SynchronizationContext currentContext = SynchronizationContext.Current;
    try
    {
      await Task.Run(async () =>
      {
        await bluetoothSocket.ConnectAsync();
        if (bluetoothSocket.IsConnected)
        {
          connected = true;
          currentContext.Post((e) =>
          {
            if (BluetoothConnectionReceived != null)
              BluetoothConnectionReceived(this, new BluetoothConnectionReceivedEventArgs(bluetoothDevice.Address, bluetoothDevice.Name, connected));

          }, null);
        }
      });
    }
    catch(Java.IO.IOException ex)
    {
      connected = false;
      currentContext.Post((e) =>
      {
        if (BluetoothConnectionReceived != null)
          BluetoothConnectionReceived( this, new BluetoothConnectionReceivedEventArgs(bluetoothDevice.Address, bluetoothDevice.Name, connected, ex));
      }, null);
      Log.Error(TAG, "Bluetooth Socket Connection Failed", ex);
    }
  }
}

/*
  BluetoothService.SerialPort is just the standard string "00001101-0000-1000-8000-00805f9b34fb" for all SPP devices.

  Xamarim supports ConnectAsync so I suggest you use it, rather than the synchronous Connect.

  Note the code above is from a BTReceiver class. I'd suggest you do something similar, as that
  way, you can reuse the code for any SPP device such as your scanner or any other SPP devices,
  whether the device is a read only device or read/write device. Then all you need to add are
  methods that handle reading and writing to the socket. You will then need a Motorola CS3070
  interpreter class that handles the specific responses from your Read thread of your BTReceiver
  class. If your class uses a SynchronizationContent to post the responses then you can get rid
  of all that RunOnUiThread stuff on your Mainactivity.
*/
