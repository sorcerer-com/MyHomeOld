namespace MyHome.TcpConnection
{
    public enum ECommandType
    {
        Invalid = 0,
        OK = 1,
        // Service Manager
        GetAvailableServices = 2,
        // PC Control
        SetKey = 3,
        GetMousePosition = 4,
        SetMousePosition = 5,
        SetMouseButton = 6,
        SetMouseWheel = 7,
        GetScreenImage = 8,
        GetCameraImage = 9,
        // Home Control
        GetLayout = 10,
        GetRooms = 11,
        // TV Control
        GetTelevisions = 12,
        SetRemoteControlButton = 13,
        GetMovies = 14,
        SetMovie = 15,
        GetImages = 16,
        SetImage = 17
    }
}
