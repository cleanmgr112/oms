namespace OMS.Model.Account
{
    public class UserModel : ModelBase
    {
        public string UserName { get; set; }
        public string UserPwd { get; set; }
        public string Salt { get; set; }
        public int State { get; set; }
    }
}
