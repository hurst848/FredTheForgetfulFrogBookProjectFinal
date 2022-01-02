using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ConnectToBluetooth : MonoBehaviour
{
	struct BLE_ScannedItem
	{
		public string RSSI;
		public string Address;
		public string Name;
	}

    public class Characteristic
    {
        public string ServiceUUID;
        public string CharacteristicUUID;
        public bool Found;
    }

    public static List<Characteristic> Characteristics = new List<Characteristic>
    {
        new Characteristic { ServiceUUID = "705CC7B4-AD8D-40BA-9583-FCB7819ED284", CharacteristicUUID = "705CC7B4-AD8D-40BA-9583-FCB7819ED284", Found = false },
    };

    public Characteristic GetCharacteristic(string serviceUUID, string characteristicsUUID)
    {
        return Characteristics.Where(c => IsEqual(serviceUUID, c.ServiceUUID) && IsEqual(characteristicsUUID, c.CharacteristicUUID)).FirstOrDefault();
    }

    public AudioSource SourceA;
    public AudioSource SourceB;

    public List<AudioClip> soundTracks;

    private bool currentSource = true; // true = SourceA | false = SourceB

    public float fadeTime = 3.0f;


    static BLE_ScannedItem connectedItem;
	static int Page_Number = -1;
    private int lastPageNumber;

	public Dropdown dropdown;
	public Button connectButton;

    public Text displayPage;
    public Button PageButton;

	public List<string> possibleNames;

	private List<string> detectedAdresses;

    private float _timeout;
    private float _startScanTimeout = 10f;
    private float _startScanDelay = 0.5f;
    private bool _startScan = true;
    private List<BLE_ScannedItem> _scannedItems;


    public bool exit = false;
    public bool paused = false;

    private bool connectedToDevice = false;
    // Start is called before the first frame update
    void Start()
    {
		detectedAdresses = new List<string>();
		dropdown.ClearOptions();

		BluetoothLEHardwareInterface.Log("Start");
        _scannedItems = new List<BLE_ScannedItem>();

        BluetoothLEHardwareInterface.Initialize(true, false, () => {

            _timeout = _startScanDelay;
        },
        (error) => {

            BluetoothLEHardwareInterface.Log("Error: " + error);

            if (error.Contains("Bluetooth LE Not Enabled"))
                BluetoothLEHardwareInterface.BluetoothEnable(true);
        });
    }

    void pauseSound()
    {
        if (paused) { paused = false; }
        else { paused = true; }
    }

    // Update is called once per frame
    void Update()
    {
        if (!connectedToDevice)
        {
            if (_timeout > 0f)
            {
                _timeout -= Time.deltaTime;
                if (_timeout <= 0f)
                {
                    if (_startScan)
                    {
                        _startScan = false;
                        _timeout = _startScanTimeout;

                        BluetoothLEHardwareInterface.ScanForPeripheralsWithServices(null, null, (address, name, rssi, bytes) =>
                        {

                            BluetoothLEHardwareInterface.Log("item scanned: " + address);
                            if (detectedAdresses.Contains(address))
                            {
                                BluetoothLEHardwareInterface.Log("ALREADY DETECTED");
                            }
                            else
                            {
                                BluetoothLEHardwareInterface.Log("item new: " + address);
                                BLE_ScannedItem scannedItem = new BLE_ScannedItem();


                                BluetoothLEHardwareInterface.Log("item set: " + address);

                                scannedItem.Address = address;
                                scannedItem.Name = name;
                                scannedItem.RSSI = rssi.ToString();

                                if (possibleNames.Contains(name))
                                {
                                    _scannedItems.Add(scannedItem);

                                    dropdown.ClearOptions();
                                    List<string> optionNames = new List<string>();
                                    for (int i = 0; i < _scannedItems.Count; i++)
                                    {
                                        optionNames.Add(_scannedItems[i].Name);
                                    }
                                    dropdown.AddOptions(optionNames);
                                    dropdown.RefreshShownValue();
                                }

                                detectedAdresses.Add(scannedItem.Name);



                            }
                        }, true);

                        BluetoothLEHardwareInterface.Log("THERE ARE THIS MANY ITEMS IN THE DICTIONARY \n\n\n\n\n" + _scannedItems.Count);


                    }
                    else
                    {
                        BluetoothLEHardwareInterface.StopScan();
                        _startScan = true;
                        _timeout = _startScanDelay;
                    }
                }
            } 
        }
        else
        {
            displayPage.text = Page_Number.ToString();
        }

	}

	public void ConnectToDevice()
    {
        PageButton.gameObject.SetActive(true);
		connectedItem = _scannedItems[dropdown.value];
        connectedToDevice = true;
        BluetoothLEHardwareInterface.StopScan();
        BluetoothLEHardwareInterface.ConnectToPeripheral(connectedItem.Address, null, null, (address, serviceUUID, characteristicUUID) => {

            var characteristic = GetCharacteristic(serviceUUID, characteristicUUID);
            if (characteristic != null)
            {
                BluetoothLEHardwareInterface.Log(string.Format("Found {0}, {1}", serviceUUID, characteristicUUID));

                characteristic.Found = true;

            
            }
        }, (disconnectAddress) => {
            
        });
        //BluetoothLEHardwareInterface.SubscribeCharacteristicWithDeviceAddress(connectedItem.Address, Characteristics[0].ServiceUUID, Characteristics[0].CharacteristicUUID, null, (deviceAddress, characteristric, bytes) => {

        //    Page_Number = int.Parse(BitConverter.ToString(bytes));
        //});

        lastPageNumber = Page_Number;
		dropdown.gameObject.SetActive(false);
		connectButton.gameObject.SetActive(false);


        StartCoroutine(runAudioStuff());
	}

    public void UpdateButtonText()
    {
        displayPage.text = Page_Number.ToString(); 
    }

    public IEnumerator fadeOutAudio(AudioSource _source, float _timeToFade)
    {
        float precision = 100.0f;
        float incrmnt = _timeToFade / precision;

        for (int i = 0; i< precision; i++)
        {
            _source.volume -= 1.0f / precision;
            yield return new WaitForSeconds(incrmnt);
        }

        _source.Pause();
        yield return null;
    }
    public IEnumerator fadeInAudio(AudioSource _source, float _timeToFade)
    {
        _source.Play();

        float precision = 100.0f;
        float incrmnt = _timeToFade / precision;

        for (int i = 0; i < precision; i++)
        {
            _source.volume += 1.0f / precision;
            yield return new WaitForSeconds(incrmnt);
        }

        yield return null;
    }

    private IEnumerator runAudioStuff()
    {

        while (!exit)
        {
            BluetoothLEHardwareInterface.ReadCharacteristic(connectedItem.Address, Characteristics[0].ServiceUUID, Characteristics[0].CharacteristicUUID, (characteristic, bytes) =>
            {
                BluetoothLEHardwareInterface.Log("Charateristic read: \n\n\n\n\n");

                string test = System.Text.Encoding.Default.GetString(bytes);
                BluetoothLEHardwareInterface.Log(test);
                BluetoothLEHardwareInterface.Log("\n\n");

                Page_Number = int.Parse(test);
            });
            if (!paused)
            {
                if (Page_Number != lastPageNumber && Page_Number != -1 && Page_Number <= soundTracks.Count && Page_Number > 0)
                {
                    lastPageNumber = Page_Number;
                    if (currentSource) // SourceA is currently playing
                    {
                        StartCoroutine(fadeOutAudio(SourceA, fadeTime));
                        SourceB.clip = soundTracks[Page_Number];
                        StartCoroutine(fadeInAudio(SourceB, fadeTime));
                        currentSource = false;
                    }
                    else // SourceB is currently playing
                    {
                        StartCoroutine(fadeOutAudio(SourceB, fadeTime));
                        SourceA.clip = soundTracks[Page_Number];
                        StartCoroutine(fadeInAudio(SourceA, fadeTime));
                        currentSource = true;
                    }
                }
                else if (Page_Number == -1 && (SourceA.clip != null && SourceB.clip != null))
                {
                    if (currentSource)
                    {
                        StartCoroutine(fadeOutAudio(SourceA, fadeTime));
                    }
                    else
                    {
                        StartCoroutine(fadeOutAudio(SourceB, fadeTime));
                    }
                }
                
            }

            yield return new WaitForSeconds(fadeTime);
        }

        yield return null;
    }
    bool IsEqual(string uuid1, string uuid2)
    {
        return (uuid1.ToUpper().CompareTo(uuid2.ToUpper()) == 0);
    }
    private void OnApplicationQuit()
    {
        BluetoothLEHardwareInterface.DisconnectPeripheral(connectedItem.Address, (address) => {
            // since we have a callback for disconnect in the connect method above, we don't
            // need to process the callback here.
        });
    }
}
