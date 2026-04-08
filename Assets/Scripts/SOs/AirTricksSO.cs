using UnityEngine;

[CreateAssetMenu(fileName = "Air Tricks Settings", menuName = "Scriptable Objects/Air Tricks Settings")]
public class AirTricksSO : ScriptableObject
{
    // ============================================================
    // TOGGLE TRICKS
    // ============================================================
    [Header("Toggle Tricks")]
    [SerializeField] private bool _canUp = true;
    public bool CanUp => _canUp;

    [SerializeField] private bool _canDown = true;
    public bool CanDown => _canDown;

    [SerializeField] private bool _canRight = false;
    public bool CanRight => _canRight;

    [SerializeField] private bool _canLeft = false;
    public bool CanLeft => _canLeft;

    // ============================================================
    // TRICK SETTINGS
    // ============================================================
    [Header("Trick Settings")]
    [Tooltip("Upward force applied during up trick")]
    [SerializeField] private float _upTrickForce = 500f;
    public float UpTrickForce => _upTrickForce;

    [Tooltip("Duration of the up trick")]
    [SerializeField] private float _upTrickTime = 0.01f;
    public float UpTrickTime => _upTrickTime;

    [Tooltip("Gravity modifier applied during up trick")]
    [SerializeField] private float _upTrickGravityMod = 0.4f;
    public float UpTrickGravityMod => _upTrickGravityMod;

    [Tooltip("Downward force applied during down trick")]
    [SerializeField] private float _downTrickForce = 6000f;
    public float DownTrickForce => _downTrickForce;

    [Tooltip("Duration of the down trick")]
    [SerializeField] private float _downTrickTime = 0.01f;
    public float DownTrickTime => _downTrickTime;

    [Tooltip("Time window to perform a clean trick")]
    [SerializeField] private float _cleanTime = 1f;
    public float CleanTime => _cleanTime;

    [Tooltip("Detection range buff applied during a clean")]
    [SerializeField] private float _cleanBuff = 500000f;
    public float CleanBuff => _cleanBuff;

    [Tooltip("Gravity modifier applied during a clean")]
    [SerializeField] private float _cleanGravityMod = 0.3f;
    public float CleanGravityMod => _cleanGravityMod;

    [Tooltip("Duration of the left trick animation")]
    [SerializeField] private float _leftTrickTime = 0.75f;
    public float LeftTrickTime => _leftTrickTime;

    [Tooltip("Minimum Y distance from ground required to perform a trick")]
    [SerializeField] private float _minYDistance = 0.1f;
    public float MinYDistance => _minYDistance;
}
