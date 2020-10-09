namespace OMS.Model
{
    /// <summary>
    /// 配置类
    /// </summary>
    public class AppSettings
    {
        public AppSettings()
        {
            CookieTimeout = 8;// 8小时
            CookieIsPersistent = false;//cookie持续性
        }
        public int CookieTimeout { get; set; }

        public bool CookieIsPersistent { get; set; }
    }
}
