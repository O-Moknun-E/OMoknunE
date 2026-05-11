[System.Serializable]
public class RoomData
{

    //[System.Serializable]
    //public class Roomdata
    //{
    //    public string roomName;
    //    public bool isPrivate;
    //    public string password;

    //}

    //public string roomName;
    //public int type; // 0 = Public, 1 = Private

   
        public string roomName;
        public int type; // 0 = Public, 1 = Private
        

        public RoomData(string name, int type)
        {
            this.roomName = name;
            this.type = type;
            
        }
    }



