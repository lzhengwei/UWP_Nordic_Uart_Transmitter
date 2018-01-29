using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Storage.Streams;
using Windows.Devices.Enumeration;
using Windows.Devices.Bluetooth;
using System.Collections.ObjectModel;
using Windows.UI.Popups;
using Windows.Security.Cryptography;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using System.Threading.Tasks;
// 空白頁項目範本已記錄在 https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x404

namespace App1
{
    /// <summary>
    /// 可以在本身使用或巡覽至框架內的空白頁面。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        BluetoothLEAdvertisementPublisher publisher = new BluetoothLEAdvertisementPublisher();
        BluetoothLEAdvertisementWatcher bleWatvher = new BluetoothLEAdvertisementWatcher();
        BluetoothLEDevice bleDevice;
        DeviceWatcher devWatcher = DeviceInformation.CreateWatcher(BluetoothLEDevice.GetDeviceSelector(), null);
        List<String> address = new List<string>(50);
        String addresslist = "";

        IReadOnlyList<GattDeviceService> myGATTservicelist;

        String UUID_SERV = "6e400001-b5a3-f393-e0a9-e50e24dcca9e";
        String UUID_WRITE = "6e400002-b5a3-f393-e0a9-e50e24dcca9e";
        String UUID_READ = "6e400003-b5a3-f393-e0a9-e50e24dcca9e"; // 0: disable,1: enable
        String receive_Text = "";
        GattDeviceService UARTservice;
        GattCharacteristic WRITEcharacteristic, READcharacteristic;

        private List<BluetoothDevice> listOfBledevice = new List<BluetoothDevice>();
        private List<String> listOfBledeviceName = new List<String>();

        int receivetimes = 0;
        public MainPage()
        {
            this.InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            listOfBledeviceName.Clear();
            textbox.Text = addresslist;
            addresslist = "";
            //advertisment_init();

            Bloos = new ObservableCollection<BluetoothDevice>();
            startScanner();
        }
        private void advertisment_init()
        {
            // Query for extra properties you want returned
            string[] requestedProperties = { "System.Devices.Aep.DeviceAddress", "System.Devices.Aep.IsConnected" };

            DeviceWatcher deviceWatcher =
                        DeviceInformation.CreateWatcher(
                                BluetoothLEDevice.GetDeviceSelector(),
                                requestedProperties,
                                DeviceInformationKind.AssociationEndpoint);
            /*    deviceWatcher.Added += DeviceWatcher_Added;
                deviceWatcher.Updated += DeviceWatcher_Updated;
                deviceWatcher.Removed += DeviceWatcher_Removed;

                // EnumerationCompleted and Stopped are optional to implement.
                deviceWatcher.EnumerationCompleted += DeviceWatcher_EnumerationCompleted;
                deviceWatcher.Stopped += DeviceWatcher_Stopped;*/

            // Start the watcher.
            deviceWatcher.Start();
        }
        async void ConnectDevice(ulong address)
        {
            // Note: BluetoothLEDevice.FromIdAsync must be called from a UI thread because it may prompt for consent.
            try
            {
                bleDevice = await BluetoothLEDevice.FromBluetoothAddressAsync(address);
                myGATTservicelist = bleDevice.GattServices;
                UARTservice = bleDevice.GetGattService(Guid.Parse(UUID_SERV));
                var characteristic = UARTservice.GetAllCharacteristics();
                READcharacteristic = characteristic[0];
                WRITEcharacteristic = characteristic[1];

                await READcharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(
                        GattClientCharacteristicConfigurationDescriptorValue.Notify);

                READcharacteristic.ValueChanged += Uart_Receive;
                textbox.Text = "Connect to " + bleDevice.Name + " Success\n" + "Address : " + address;
            }
            catch (Exception xx)
            {
                addresslist = xx.ToString();
                textbox.Text = "Connect to " + bleDevice.Name + " Error\nPlease check Bluetooth Device status and is paired to computer.";
            }
            /*if (bluetoothLeDevice.ConnectionStatus == BluetoothConnectionStatus.Connected)
            {
                var dialog = new MessageDialog(bluetoothLeDevice.Name);
                await dialog.ShowAsync();
            }*/
            // ...
            //addresslist += deviceInfo.Name + " " + deviceInfo.Id + "\n";
        }
        public ObservableCollection<BluetoothDevice> Bloos
        {
            get;
            set;
        }
        public void startScanner()
        {
            string[] requestedProperties = { "System.Devices.Aep.DeviceAddress", "System.Devices.Aep.IsConnected" };

            DeviceWatcher deviceWatcher =
                        DeviceInformation.CreateWatcher(
                                BluetoothLEDevice.GetDeviceSelector(),
                                requestedProperties,
                                DeviceInformationKind.AssociationEndpoint);
            bleWatvher.ScanningMode = BluetoothLEScanningMode.Active;
            
            bleWatvher.Received += BleWatvher_Received;
            bleWatvher.Stopped += BleWatvher_Stopped;

            deviceWatcher.Stopped += DevWatcher_Stopped;
            deviceWatcher.Added += DevWatcher_Added;

            bleWatvher.Start();
            //deviceWatcher.Start();
        }
        private void DevWatcher_Added(DeviceWatcher sender, DeviceInformation args)
        {
            //textbox.Text += args.Name+"\n";
            addresslist += args.Name + " " + args.EnclosureLocation + "\n";
            // ConnectDevice(args);
        }
        private void DevWatcher_Stopped(DeviceWatcher sender, object args)
        {
            devWatcher.Start();
        }
        private void BleWatvher_Stopped(BluetoothLEAdvertisementWatcher sender, BluetoothLEAdvertisementWatcherStoppedEventArgs args)
        {
            bleWatvher.Start();
        }
        List<String> listOfdata = new List<String>();
        private async void BleWatvher_Received(BluetoothLEAdvertisementWatcher sender, BluetoothLEAdvertisementReceivedEventArgs args)
        {
            //textbox.Text = "gg";
            
            //addresslist += args.Advertisement.LocalName + " " + args.BluetoothAddress;
            //var bledevice = await BluetoothLEDevice.FromBluetoothAddressAsync(args.BluetoothAddress);
            if (args.Advertisement.LocalName != "" && !listOfBledeviceName.Contains(args.BluetoothAddress + ""))
            {
                address.Add(args.Advertisement.LocalName);
                
                listOfBledeviceName.Add(args.BluetoothAddress+"");
                listOfBledevice.Add(new BluetoothDevice { Address = args.BluetoothAddress, Name = args.Advertisement.LocalName,SignalStrength= args.RawSignalStrengthInDBm });
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    listview1.ItemsSource = null;
                    listview1.ItemsSource = listOfBledevice;
                });

            }


        }

        private void Button2_Click(object sender, RoutedEventArgs e)
        {
            textbox.Text = addresslist;
  


        }

        private async void Uart_Button_Click(object sender, RoutedEventArgs e)
        {
            MessageDialog showDialog = new MessageDialog("Hi Welcome to Windows 10");

            showDialog.Commands.Add(new UICommand("Start") { Id = 0 }); //send "11"

            showDialog.Commands.Add(new UICommand("Stop") { Id = 1 });  //send "22"

            showDialog.Commands.Add(new UICommand("cancel") { Id = 2 });
            showDialog.DefaultCommandIndex = 0;

            showDialog.CancelCommandIndex = 1;

            var result = await showDialog.ShowAsync();
            string sendtext = "";
            switch ((int)result.Id)
            {
                case 0:
                    sendtext = "11";
                    receive_Text = "";
                    receivetimes = 0;
                    break;
                case 1:
                    sendtext = "22";
                    textbox.Text = receive_Text;
                    break;
                case 2:
                    break;
            }
            if((int)result.Id<2)
            {
                try
                {
                    var writeBuffer = CryptographicBuffer.ConvertStringToBinary(sendtext,
                                        BinaryStringEncoding.Utf8);
                    var writeSuccessful = await WRITEcharacteristic.WriteValueAsync(writeBuffer);
                }
                catch (Exception x)
                {
                    textbox.Text = "Send message Error\nPlease check Bluetooth Device status and is paired to computer.\n\n"+ x.ToString();
                }
                
            }
          
        }
        byte[] compareinput=new byte[18];
        void Uart_Receive(GattCharacteristic sender,
                                    GattValueChangedEventArgs args)
        {
            // An Indicate or Notify reported that the value has changed.
            var reader = DataReader.FromBuffer(args.CharacteristicValue);
            byte[] input = new byte[reader.UnconsumedBufferLength];
            reader.ReadBytes(input);
            if (!compareinput.SequenceEqual(input) )
            {
                receivetimes++;
                //receive_Text += input[0].ToString()+ input[1].ToString() + "\n";
                for (int i = 0; i < 18; i++)
                {
                    int high = input[i] / 16, low = input[i] & 0x0f;
                    if (high > 0x09)
                        high = high + 0x37;
                    else
                        high = high + 0x30;

                    if (low > 0x09)
                        low = low + 0x37;
                    else
                        low = low + 0x30;
                    char H = (char)high, L = (char)low;
                    receive_Text += H + "" + L; //*/
                                                /* char t = (char)input[i];
                                                 receive_Text += t; // */
                }
            }
            compareinput = input;
            UpdateUI();
        }
        int listviewSelectindex = -1;
        private void listview1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            listviewSelectindex = listview1.SelectedIndex;
            ConnectDevice(listOfBledevice[listviewSelectindex].Address);
        }
        private void listview1_ItemClick(object sender, ItemClickEventArgs e)
        {
            if(listviewSelectindex== listview1.SelectedIndex && listview1.SelectedIndex!=-1)
                ConnectDevice(listOfBledevice[listviewSelectindex].Address);
        }
        private void Uart_Button_send_Click(object sender, RoutedEventArgs e)
        {
            Messagebox_textbox_show("Send text to BLE device");
        }
        private async Task InputTextDialogAsync(string title)
        {
            TextBox inputTextBox = new TextBox();
            inputTextBox.AcceptsReturn = false;
            inputTextBox.Height = 32;
            ContentDialog dialog = new ContentDialog();
            dialog.Content = inputTextBox;
            dialog.Title = title;
            dialog.IsSecondaryButtonEnabled = true;
            dialog.PrimaryButtonText = "Send";
            dialog.SecondaryButtonText = "Cancel";

            if (await dialog.ShowAsync() == ContentDialogResult.Primary)
            {
                try
                {
                    var writeBuffer = CryptographicBuffer.ConvertStringToBinary(inputTextBox.Text,
                        BinaryStringEncoding.Utf8);
                    var writeSuccessful = await WRITEcharacteristic.WriteValueAsync(writeBuffer);
                }
                catch (Exception x)
                {
                    textbox.Text = "Send message Error\nPlease check Bluetooth Device status and is paired to computer.\n\n" + x.ToString();
                }
            }
  
        }
        private async void Messagebox_textbox_show(string title)
        {
           await InputTextDialogAsync(title);
           
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            textbox.Text = "";
        }



        private async void UpdateUI()
        {
           
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
            () =>
            {
                textbox.Text = receive_Text;
            });
        }
    }
    public class BluetoothDevice
    {
        public ulong Address { get; set; }
        public string Name{ get; set; }
        public short SignalStrength { get; set; }
    }
}
