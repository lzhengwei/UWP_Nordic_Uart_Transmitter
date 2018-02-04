# UWP Nordic Uart Transmitter

This is a Windows10 UWP C# project.

Goal of this project is connect to Nordic IC nrf51822(MDBT40) through Ble(Bluetooth Low Energy).

This program has basic uart function include **list and connect to ble device**、**list Ble device's UUID**、**send and receive data***

Divide into two project

| Project  | function |
| ---------- | ----------- |
| BLE_GATT_list  | List all Ble divice GATT UUID |
| BLE_uart_connector | As a connector to Ble divice, Use Nordic Uart UUID to sned/receive data form device |

# Artitecture 

Nrf51822(MDBT40) connect to android or windows10 UWP application send raw data through Bluetooth Low Energy(BLE).

(I have another project for Nrf51822 program which develope on keilC5 name as Nrf51822 with LSM9DS)

LSM9DS、LIS3DH and Micro SD card connect to Nrf51822 through SPI protcol.

![artitecture](https://github.com/lzhengwei/UWP_Nordic_Uart_Transmitter/blob/master/Structure.jpg)

# Connect Step

First, click Button name as **Scanning BLE Devices**. Then there will list Ble device at listview right at button.

Second, click listview item which you want to connect. If connect successful, it will show some text contain "connect succcess".

If connect fail, Make sure the ble device you want connect is pairing. You could check this on your bluetooth setting board. 

When connect to ble device first time, It always need to pair ble device manual. 

# Main fuction in program

Most of this pragram is from [Windows Dev center->Bluetooth GATT Client](https://docs.microsoft.com/en-us/windows/uwp/devices-sensors/gatt-client). 

There has ble connect step code completely. I just add and fix some program to make it better and easier to use.

## Scanning and Connect

The program will always scanning when executing. If there find ble device, It will trigger **BleWatvher_Received** function.

In that function I edit it to below code.

***BleWatvher_Received***
```c#
if (args.Advertisement.LocalName != "" && !listOfBledeviceName.Contains(args.BluetoothAddress + "")) //check if there not exist same address
{
    address.Add(args.Advertisement.LocalName);
                
    listOfBledeviceName.Add(args.BluetoothAddress+""); //add address for check.
    listOfBledevice.Add(new BluetoothDevice { Address = args.BluetoothAddress, Name = args.Advertisement.LocalName,SignalStrength=args.RawSignalStrengthInDBm }); //add and sotre device's name、address、rms
    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
    {
       listview1.ItemsSource = null;
       listview1.ItemsSource = listOfBledevice; //refresh list view
    });

}
```
When click item on listview it will execute **ConnectDevice** function.

***ConnectDevice***
```c#
bleDevice = await BluetoothLEDevice.FromBluetoothAddressAsync(address); //connect ble device through address

UARTservice = bleDevice.GetGattService(Guid.Parse(UUID_SERV)); //Get Ble device service from UUID_SERV, It is define at Nordic uuids. Nedd get service first, then get Characteristics under service.
var characteristic = UARTservice.GetAllCharacteristics(); //Get Characteristics under Uart service
READcharacteristic = characteristic[0]; //Get Uart Read characteristic, Could check UUID name. It is same as Nordice define Uart read/write UUIDs.
WRITEcharacteristic = characteristic[1];//Get Uart Write characteristic

awaitREADcharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify); //READ characteristic need to enable notify. Then could get receive event every time.
READcharacteristic.ValueChanged += Uart_Receive;
textbox.Text = "Connect to " + bleDevice.Name + " Success\n" + "Address : " + address; //connect success.
```

## Write/Read value from BLE uart
### Write

```c#
var writeBuffer = CryptographicBuffer.ConvertStringToBinary(inputTextBox.Text,BinaryStringEncoding.Utf8); //Converte string to binary into writeBuffer
var writeSuccessful = await WRITEcharacteristic.WriteValueAsync(writeBuffer); //parse value to WRITE characteristic. Then it will send value.
```

### Receive

```c#
var reader = DataReader.FromBuffer(args.CharacteristicValue); //Get value from Characteristic
byte[] input = new byte[reader.UnconsumedBufferLength];
reader.ReadBytes(input); //Characteristic value to byte
if (!compareinput.SequenceEqual(input)) //I have some problem here. When nrf51822 send value to computer, It will triger receive event twice. So, I use an static byte array to check the vaule is same or not. It is so strange, Maybe I lost some other code at here.
{
 /*at here, Could transform byte to array or just show all value on UI. In my case, I trasform acceleromter raw data to string(byte to hex) in project*/
 compareinput = input; // save last byte array .
}
```

Here just simply introduce some key function at ble-uart.

If you want learn more about all project work or other problems, could send E-mail to contact me.

I'd be glad to help if you don't mind my bad English.
