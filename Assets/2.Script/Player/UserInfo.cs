using System.Collections.Generic;
using System;
public class UserFullData {
    public UserInfo userInfo;
    public UserData userData;
}

public class UserInfo {
    public int? userNo;
    public string userId;
    public string userName;
    public string platform;
    public string userEmail;
    public string userUuid;
    public string lastLgin;
    public string lastLgout;
    public string lastTicketChargeTime;
}

public class UserData {
    public int? userTicket;
    public int? userMoney;
    public string userCharList;
    public List<ProductItem> userCharDataList;
    public int? userSelectChar;
    public string timeTicketEndDate;
    public string uniqProdList;
}
