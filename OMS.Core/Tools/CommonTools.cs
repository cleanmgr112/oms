using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using RazorLight;
using System;
using System.Collections.Generic;
using System.Data;
using System.DrawingCore;
using System.DrawingCore.Drawing2D;
using System.DrawingCore.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OMS.Core.Tools
{
    public class CommonTools
    {
        #region encrypt
        //默认密钥向量
        private static byte[] _aesKey = { 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF, 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCC, 0xEF };

        /// <summary>
        /// AES加密算法
        /// </summary>
        /// <param name="plainText">明文字符串</param>
        /// <returns>将加密后的密文转换为Base64编码，以便显示</returns>
        public static string AESEncrypt(string plainText, string Key)
        {
            //分组加密算法
            SymmetricAlgorithm des = Rijndael.Create();
            byte[] inputByteArray = Encoding.UTF8.GetBytes(plainText);//得到需要加密的字节数组
            //设置密钥及密钥向量
            des.Key = Encoding.UTF8.GetBytes(Key);
            des.IV = _aesKey;
            byte[] cipherBytes = null;
            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(inputByteArray, 0, inputByteArray.Length);
                    cs.FlushFinalBlock();
                    cipherBytes = ms.ToArray();//得到加密后的字节数组
                    cs.Close();
                    ms.Close();
                }
            }
            return Convert.ToBase64String(cipherBytes);
        }

        /// <summary>
        /// AES解密
        /// </summary>
        /// <param name="cipherText">密文字符串</param>
        /// <returns>返回解密后的明文字符串</returns>
        public static string AESDecrypt(string showText, string Key)
        {
            showText = showText.Replace("%3d", "=").Replace(" ", "+").Replace("%2b", "+");

            byte[] cipherText = Convert.FromBase64String(showText);
            SymmetricAlgorithm des = Rijndael.Create();
            des.Key = Encoding.UTF8.GetBytes(Key);
            des.IV = _aesKey;
            byte[] decryptBytes = new byte[cipherText.Length];
            using (MemoryStream ms = new MemoryStream(cipherText))
            {
                using (CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Read))
                {
                    cs.Read(decryptBytes, 0, decryptBytes.Length);
                    cs.Close();
                    ms.Close();
                }
            }
            return Encoding.UTF8.GetString(decryptBytes).Replace("\0", "");   ///将字符串后尾的'\0'去掉
        }

        /// <summary>
        /// md5摘要
        /// </summary>
        /// <param name="plain"></param>
        /// <param name="isMd5"></param>
        /// <returns></returns>
        public static string Md5Hash(string plain, string format = "X2")
        {
            MD5 md5 = MD5CryptoServiceProvider.Create();
            byte[] result = md5.ComputeHash(UTF8Encoding.UTF8.GetBytes(plain));
            StringBuilder sb = new StringBuilder();
            foreach (var i in result)
                sb.Append(i.ToString(format));
            return sb.ToString();
        }
        #endregion

        /// <summary>
        /// 生成随机字符串
        /// </summary>
        /// <param name="length">长度</param>
        /// <returns></returns>
        public static string CreateRandomStr(int length, RandomType randomType = RandomType.Mix)
        {
            var digitArray = new string[] { "2", "3", "4", "5", "6", "7", "8", "9" };//去除0，1
            var letterArray = new string[] { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "m", "n", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z" };//小写字母，去除l、o
            var capArray = letterArray.Select(i => i.ToUpper());//大写字母
            IList<string> randomArray;
            switch (randomType)
            {
                case RandomType.Digit:
                    randomArray = digitArray;
                    break;
                case RandomType.Letter:
                    randomArray = letterArray.Concat(capArray).ToList();
                    break;
                default:
                    randomArray = digitArray.Concat(letterArray).Concat(capArray).ToList();
                    break;
            }
            var random = new Random();
            var builder = new StringBuilder();
            //生成随机字符串
            for (int i = 0; i < length; i++)
            {
                builder.Append(randomArray[random.Next(randomArray.Count)]);
            }
            return builder.ToString();
        }

        /// <summary>
        /// 创建验证码的图片
        /// </summary>
        /// <param name="validateCode">验证码内容</param>
        /// <returns></returns>
        public static byte[] CreateValidateGraphic(string validateCode)
        {
            Bitmap image = new Bitmap((int)Math.Ceiling(validateCode.Length * 15.0), 25);
            Graphics g = Graphics.FromImage(image);
            try
            {
                //生成随机生成器
                Random random = new Random();
                //清空图片背景色
                g.Clear(Color.White);
                //画图片的干扰线
                for (int i = 0; i < 25; i++)
                {
                    int x1 = random.Next(image.Width);
                    int x2 = random.Next(image.Width);
                    int y1 = random.Next(image.Height);
                    int y2 = random.Next(image.Height);
                    g.DrawLine(new Pen(Color.Silver), x1, y1, x2, y2);
                }
                Font font = new Font("Arial", 12, (FontStyle.Bold | FontStyle.Italic));
                LinearGradientBrush brush = new LinearGradientBrush(new Rectangle(0, 0, image.Width, image.Height),
                 Color.Blue, Color.DarkRed, 1.2f, true);
                g.DrawString(validateCode, font, brush, 3, 2);
                //画图片的前景干扰点
                for (int i = 0; i < 100; i++)
                {
                    int x = random.Next(image.Width);
                    int y = random.Next(image.Height);
                    image.SetPixel(x, y, Color.FromArgb(random.Next()));
                }
                //画图片的边框线
                g.DrawRectangle(new Pen(Color.Silver), 0, 0, image.Width - 1, image.Height - 1);
                //保存图片数据
                MemoryStream stream = new MemoryStream();
                image.Save(stream, ImageFormat.Jpeg);
                //输出图片流
                return stream.ToArray();
            }
            finally
            {
                g.Dispose();
                image.Dispose();
            }
        }

        /// <summary>
        /// 编译Razor模板文件，生成字符串
        /// </summary>
        /// <remarks>
        /// 开源插件：RazorLight
        /// </remarks>
        /// <param name="key">缓存key</param>
        /// <param name="model">Model</param>
        /// <param name="viewbag">ViewBag</param>
        /// <returns></returns>
        public static string RunRazorTemplate(string key, string plain, object model = null, dynamic viewbag = null)
        {
            var engine = new RazorLightEngineBuilder().UseMemoryCachingProvider().Build();
            Type modelType = typeof(Nullable);
            if (model != null)
            {
                modelType = model.GetType();
            }
            return engine.CompileRenderAsync(key, plain, model, modelType, viewbag).Result;
        }

        #region excel
        /// <summary>
        /// 读取excel
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="dropTitle">舍弃标头</param>
        /// <returns></returns>
        public static DataTable ReadExcel(Stream stream, bool dropTitle = true)
        {
            try
            {
                var result = new DataTable();

                var hssfWorkBook = new HSSFWorkbook(stream);

                var sheet = hssfWorkBook.GetSheetAt(0);
                var rows = sheet.GetRowEnumerator();
                var rowNum = sheet.GetRow(0).LastCellNum;
                for (int j = 0; j < rowNum; j++)
                {
                    result.Columns.Add();
                }
                if (dropTitle)
                {
                    rows.MoveNext();
                }
                while (rows.MoveNext())
                {
                    HSSFRow row = rows.Current as HSSFRow;
                    DataRow dr = result.NewRow();
                    for (int i = 0; i < rowNum; i++)
                    {
                        var cell = row.GetCell(i);
                        if (cell == null)
                        {
                            dr[i] = null;
                        }
                        else
                        {
                            dr[i] = cell.ToString();
                        }
                    }
                    result.Rows.Add(dr);
                }
                return result;
            }
            catch
            {
                return null;
            }
        }

        public static Stream WriteExcel(DataTable table, int[] widthArray = null)
        {
            try
            {
                MemoryStream ms = new MemoryStream();
                using (table)
                {
                    IWorkbook workbook = new HSSFWorkbook();
                    if (table.TableName == "" || table.TableName == null)
                    {
                        table.TableName = "sheet0";
                    }
                    var defaultStyle = workbook.CreateCellStyle();
                    var defaultFont = workbook.CreateFont();
                    defaultFont.FontName = "宋体";//字体样式
                    defaultFont.FontHeightInPoints = 11;//字体大小
                    defaultStyle.WrapText = true;//自动换行
                    defaultStyle.VerticalAlignment = VerticalAlignment.Center;//垂直居中
                    defaultStyle.SetFont(defaultFont);

                    ISheet sheet = workbook.CreateSheet(table.TableName);
                    sheet.DefaultRowHeight = 2 * 256;//行高

                    IRow headerRow = sheet.CreateRow(0);
                    var headStyle = workbook.CreateCellStyle();
                    headStyle.CloneStyleFrom(defaultStyle);
                    var headFont = workbook.CreateFont();
                    headFont.FontName = "宋体";//字体样式
                    headFont.FontHeightInPoints = 11;//字体大小
                    headFont.IsBold = true;
                    headStyle.SetFont(headFont);
                    headerRow.RowStyle = headStyle;

                    var isSetWidth = widthArray != null;
                    foreach (DataColumn column in table.Columns)//列名
                    {
                        var cell = headerRow.CreateCell(column.Ordinal);
                        if (isSetWidth)
                        {
                            sheet.SetColumnWidth(column.Ordinal, widthArray[column.Ordinal] * 256);//列宽
                        }
                        else
                        {
                            sheet.SetColumnWidth(column.Ordinal, 20 * 256);//列宽
                        }
                        cell.CellStyle = headStyle;
                        if (string.IsNullOrEmpty(column.ColumnName))
                        {
                            cell.SetCellValue("列" + column.Ordinal);
                        }
                        else
                        {
                            cell.SetCellValue(column.ColumnName);
                        }
                    }
                    int rowIndex = 1;
                    foreach (DataRow row in table.Rows)
                    {
                        IRow dataRow = sheet.CreateRow(rowIndex);
                        foreach (DataColumn column in table.Columns)
                        {
                            var cell = dataRow.CreateCell(column.Ordinal);
                            cell.CellStyle = defaultStyle;
                            cell.SetCellValue(row[column].ToString());
                        }
                        rowIndex++;
                    }


                    workbook.Write(ms);
                    ms.Flush();
                    ms.Position = 0;
                    return ms;
                }
            }
            catch(Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// datatable转成excel 流
        /// </summary>
        /// <param name="table"></param>
        /// <param name="widths"></param>
        /// <param name="sheetName"></param>
        /// <returns></returns>
        public static MemoryStream Convert2ExcelStream( DataTable table, int[] widths = null, string sheetName = "sheet1")
        {
            IWorkbook workbook = new HSSFWorkbook();
            ISheet sheet = workbook.CreateSheet(sheetName);
            IRow dataRow = null;
            ICell cell = null;
            int columnIndex = 0;
            if (widths != null)//列宽
            {
                foreach (var width in widths)
                {
                    sheet.SetColumnWidth(columnIndex++, width * 2560);
                }
                columnIndex = 0;
            }
            if (table.Rows != null && table.Rows.Count > 0)//表数据
            {
                int rowIndex = 0;
                int columnCount = table.Columns.Count;
                foreach (DataRow row in table.Rows)
                {
                    dataRow = sheet.CreateRow(rowIndex++);
                    while (columnIndex < columnCount)
                    {
                        cell = dataRow.CreateCell(columnIndex++);
                        cell.SetCellValue(row[columnIndex - 1].ToString());
                    }
                    columnIndex = 0;
                }
            }
            MemoryStream ms = new MemoryStream();//using 报错：Cannot access a closed Stream.
            workbook.Write(ms);
            ms.Position = 0;//HACK:重点：注意流的起点位置
            return ms;
        }


        #endregion

        #region thread
        public static void ExcuteTask(Func<bool> action)
        {
            var tryTime = 0;
            bool isSuccess = false;
            var task = Task.Factory.StartNew(() =>
            {
                while (!isSuccess)
                {
                    isSuccess = action();
                    if (tryTime > 3)
                    {
                        break;
                    }
                    tryTime++;
                    if (!isSuccess)
                    {
                        Thread.Sleep(5000);
                    }
                }
                return isSuccess;
            });
        }
        #endregion

        #region io
        /// <summary>
        /// 压缩文件
        /// </summary>
        /// <param name="filepaths">要压缩文件路径</param>
        /// <returns></returns>
        public static Stream ZipFiles(params string[] filePaths)
        {
            if (filePaths == null && filePaths.Length == 0)
                return null;
            var zipStream = new MemoryStream();
            using (var zip = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
            {
                foreach (var item in filePaths)
                {
                    var entry = zip.CreateEntry(Path.GetFileName(item));
                    using (var entryStream = entry.Open())
                    {
                        using (var fileStream = new FileStream(item, FileMode.Open))
                        {
                            fileStream.CopyTo(entryStream);
                        }
                    }
                }
            }
            zipStream.Position = 0;
            return zipStream;
        }
        #endregion

        public enum RandomType
        {
            /// <summary>
            /// 纯数字
            /// </summary>
            Digit,
            /// <summary>
            /// 纯字母
            /// </summary>
            Letter,
            /// <summary>
            /// 混合
            /// </summary>
            Mix
        }

        /// <summary>
        /// 单据编号
        /// </summary>
        /// <param name="billNo">pf</param>
        /// <returns></returns>
        public static string GetSerialNumber(string billNo)
        {
            Random random = new Random();
            return billNo + DateTime.Now.ToString("yyyyMMddHHmmssfff");
        }
        /// <summary>
        /// 获取序号（head + 两位年 + 两位月 + 7位秒 + 3位毫秒）
        /// </summary>
        /// <returns></returns>
        public static string GetSeiNoByHead(string head)
        {
            DateTime end = System.DateTime.Now;
            int totalSeconds = 0;
            int year = end.Year;
            int month = end.Month;
            DateTime startdate = new DateTime(year, month, 1, 8, 0, 0);
            TimeSpan seconds = end - startdate;
            totalSeconds = Convert.ToInt32(seconds.TotalSeconds);
            int millSecond = DateTime.Now.Millisecond;
            Thread.Sleep(1);
            StringBuilder sb = new StringBuilder();
            sb.Append(head);
            sb.Append(year.ToString().Substring(2, 2));
            sb.Append(month.ToString().PadLeft(2, '0'));
            sb.Append(totalSeconds.ToString().PadLeft(7, '0'));
            sb.Append(millSecond.ToString().PadLeft(3, '0'));
            return sb.ToString();
        }
        /// <summary>
        /// 获取签名
        /// </summary>
        /// <param name="sParams"></param>
        /// <returns></returns>
        public static string getSign(SortedDictionary<string, string> sParams, string key)
        {
            string sign = string.Empty;
            StringBuilder sb = new StringBuilder();
            foreach (KeyValuePair<string, string> temp in sParams)
            {
                if (temp.Value == "" || temp.Value == null || temp.Key.ToLower() == "sign" || temp.Key.ToLower() == "data")
                {
                    continue;
                }
                sb.Append(temp.Key.Trim() + "=" + temp.Value.Trim() + "&");
            }
            sb.Append("key=" + key.Trim() + "");
            string signkey = sb.ToString();
            sign = MD5Util.GetMD5_ToUpper(signkey, "utf-8");

            return sign;
        }

        /// <summary>
        /// 拼接请求参数
        /// </summary>
        /// <param name="sParams"></param>
        /// <param name="sign"></param>
        /// <returns></returns>
        public static string getPostData(SortedDictionary<string, string> sParams, string sign)
        {
            StringBuilder sb = new StringBuilder();
            foreach (KeyValuePair<string, string> temp in sParams)
            {
                if (temp.Value == "" || temp.Value == null)
                {
                    continue;
                }
                sb.Append(temp.Key.Trim() + "=" + temp.Value.Trim() + "&");
            }
            sb.Append("sign=" + sign.Trim() + "");

            return sb.ToString();
        }

        /// <summary>
        /// post数据到指定url并返回数据
        /// </summary>
        public static string PostToUrl(string url, string postData)
        {
            System.Text.Encoding utf_encoding = System.Text.Encoding.GetEncoding("utf-8");

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";

            if (postData != null)
            {
                byte[] data = utf_encoding.GetBytes(postData);

                request.ContentLength = data.Length;

                Stream myRequestStream = request.GetRequestStream();

                // 发送数据
                myRequestStream.Write(data, 0, data.Length);
                myRequestStream.Flush();
                myRequestStream.Close();
            }

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            Stream myResponseStream = response.GetResponseStream();
            StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));

            string retString = myStreamReader.ReadToEnd();

            myStreamReader.Close();
            myResponseStream.Close();

            return retString;
        }

        /// <summary>
        /// 获取导出模板
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, string> OutPutTemplate(OutPutType outPutType) {
            Dictionary<string, string> columnNames = new Dictionary<string, string>();
            switch (outPutType)
            {
                case OutPutType.B2C:
                    columnNames = new Dictionary<string, string>(){
                    { "创建时间","CreatedTime" },
                    { "发货时间","DeliveryDate"},
                    { "店铺","ShopName" },
                    { "订单号","SerialNumber" },
                    { "收货人", "CustomerName" },
                    { "订单状态", "StateName" },
                    { "平台订单号", "PSerialNumber" },
                    { "原订单号", "OrgionSerialNumber" },
                    { "支付方式", "PayTypeName" },
                    { "昵称","UserName"},
                    { "手机号", "CustomerPhone" },
                    { "地址","Address"},
                    { "优惠前总价","SumOrginPrice" },
                    { "订单均摊金额","SumAvgPrice"},
                    { "订单已付款金额","PayPrice"},
                    { "平台积分","IntegralValue"},
                    { "平台优惠券","ProductCoupon"},
                    { "付款状态", "PayStateName" },
                    { "付款时间", "PayDate" },
                    { "快递公司","DeliveryTypeName"},
                    { "物流单号", "DeliveryNumber" },
                    { "发票类型", "InvoiceTypeName" },
                    { "发票号","InvoiceNumber"},
                    { "发票抬头","InvoiceHead"},
                    { "客服备注","AdminMark"},
                    { "订单备注","OrderMark"},
                    { "给仓库留言","ToWareHouseMark"}
                    };
                    break;
                case OutPutType.B2CDetail:
                    columnNames = new Dictionary<string, string>() {
                    { "创建时间","CreatedTime" },
                    { "发货时间","DeliveryDate"},
                    { "店铺","ShopName" },
                    { "订单号","SerialNumber" },
                    { "收货人", "CustomerName" },
                    { "商品编码", "ProductCode" },
                    { "商品名称", "ProductName" },
                    { "单价", "UnitPrice" },
                    { "数量", "Quantity" },
                    { "均摊金额", "AvgPrice" },
                    { "订单状态", "StateName" },
                    { "平台订单号", "PSerialNumber" },
                    { "原订单号", "OrgionSerialNumber" },
                    { "支付方式", "PayTypeName" },
                    { "昵称","UserName"},
                    { "手机号", "CustomerPhone" },
                    { "地址","Address"},
                    { "优惠前总价","SumOrginPrice" },
                    { "订单均摊金额","SumAvgPrice"},
                    { "订单已付款金额","PayPrice"},
                    { "平台积分","IntegralValue"},
                    { "平台优惠券","ProductCoupon"},
                    { "付款状态", "PayStateName" },
                    { "付款时间", "PayDate" },
                    { "快递公司","DeliveryTypeName"},
                    { "物流单号", "DeliveryNumber" },
                    { "发票类型", "InvoiceTypeName" },
                    { "发票号","InvoiceNumber"},
                    { "发票抬头","InvoiceHead"},
                    { "客服备注","AdminMark"},
                    { "订单备注","OrderMark"},
                    { "给仓库留言","ToWareHouseMark"},
                    { "标准价", "OrginPrice"},
                    { "仓库", "WareHouseName" },
                    { "现金支付金额", "PayMoneyPrice" }
                  };
                    break;
                case OutPutType.B2B:
                    columnNames = new Dictionary<string, string>(){
                    { "创建时间","CreatedTime" },
                    { "发货时间","DeliveryDate"},
                    { "店铺","ShopName" },
                    { "订单号","SerialNumber" },
                    { "业务员","SalesManName" },
                    { "客户","ClientName" },
                    { "客户类型","UserTypeName"},
                    { "收货人", "CustomerName" },
                    { "订单状态", "StateName" },
                    { "平台订单号", "PSerialNumber" },
                    { "原订单号", "OrgionSerialNumber" },
                    { "支付方式", "PayTypeName" },
                    { "昵称","UserName"},
                    { "手机号", "CustomerPhone" },
                    { "地址","Address"},
                    { "优惠前总价","SumOrginPrice" },
                    { "订单均摊金额","SumAvgPrice"},
                    { "订单已付款金额","PayPrice"},
                    { "平台积分","IntegralValue"},
                    { "平台优惠券","ProductCoupon"},
                    { "付款状态", "PayStateName" },
                    { "付款时间", "PayDate" },
                    { "快递公司","DeliveryTypeName"},
                    { "物流单号", "DeliveryNumber" },
                    { "发票类型", "InvoiceTypeName" },
                    { "发票号","InvoiceNumber"},
                    { "发票抬头","InvoiceHead"},
                    { "客服备注","AdminMark"},
                    { "订单备注","OrderMark"},
                    { "给仓库留言","ToWareHouseMark"}
                    };
                    break;
                case OutPutType.B2BDetail:
                    columnNames = new Dictionary<string, string>() {
                    { "创建时间","CreatedTime" },
                    { "发货时间","DeliveryDate"},
                    { "店铺","ShopName" },
                    { "订单号","SerialNumber" },
                    { "业务员","SalesManName" },
                    { "客户","ClientName" },
                    { "客户类型","UserTypeName"},
                    { "收货人", "CustomerName" },
                    { "商品编码", "ProductCode" },
                    { "商品名称", "ProductName" },
                    { "单价", "UnitPrice" },
                    { "数量", "Quantity" },
                    { "均摊金额", "AvgPrice" },
                    { "订单状态", "StateName" },
                    { "平台订单号", "PSerialNumber" },
                    { "原订单号", "OrgionSerialNumber" },
                    { "支付方式", "PayTypeName" },
                    { "昵称","UserName"},
                    { "手机号", "CustomerPhone" },
                    { "地址","Address"},
                    { "优惠前总价","SumOrginPrice" },
                    { "订单均摊金额","SumAvgPrice"},
                    { "订单已付款金额","PayPrice"},
                    { "平台积分","IntegralValue"},
                    { "平台优惠券","ProductCoupon"},
                    { "付款状态", "PayStateName" },
                    { "付款时间", "PayDate" },
                    { "快递公司","DeliveryTypeName"},
                    { "物流单号", "DeliveryNumber" },
                    { "发票类型", "InvoiceTypeName" },
                    { "发票号","InvoiceNumber"},
                    { "发票抬头","InvoiceHead"},
                    { "客服备注","AdminMark"},
                    { "订单备注","OrderMark"},
                    { "给仓库留言","ToWareHouseMark"},
                    { "标准价", "OrginPrice"},
                    { "仓库", "WareHouseName" },
                    { "现金支付金额", "PayMoneyPrice" }
                  };
                    break;
                default:
                    break;
            }
            return columnNames;
        }
    }

    /// <summary>
    /// AES加解密
    /// </summary>
    public class AESHelper
    {
        //默认密钥向量
        private static byte[] _key1 = { 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF, 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCC, 0xEF };

        /// <summary>
        /// AES加密算法
        /// </summary>
        /// <param name="plainText">明文字符串</param>
        /// <returns>将加密后的密文转换为Base64编码，以便显示</returns>
        public static string AESEncrypt(string plainText, string Key)
        {
            //分组加密算法
            SymmetricAlgorithm des = Rijndael.Create();
            byte[] inputByteArray = Encoding.UTF8.GetBytes(plainText);//得到需要加密的字节数组
            //设置密钥及密钥向量
            des.Key = Encoding.UTF8.GetBytes(Key);
            des.IV = _key1;
            byte[] cipherBytes = null;
            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(inputByteArray, 0, inputByteArray.Length);
                    cs.FlushFinalBlock();
                    cipherBytes = ms.ToArray();//得到加密后的字节数组
                    cs.Close();
                    ms.Close();
                }
            }
            return Convert.ToBase64String(cipherBytes);
        }

        /// <summary>
        /// AES解密
        /// </summary>
        /// <param name="cipherText">密文字符串</param>
        /// <returns>返回解密后的明文字符串</returns>
        public static string AESDecrypt(string showText, string Key)
        {
            showText = showText.Replace("%3d", "=").Replace(" ", "+").Replace("%2b", "+");

            byte[] cipherText = Convert.FromBase64String(showText);
            SymmetricAlgorithm des = Rijndael.Create();
            des.Key = Encoding.UTF8.GetBytes(Key);
            des.IV = _key1;
            byte[] decryptBytes = new byte[cipherText.Length];
            using (MemoryStream ms = new MemoryStream(cipherText))
            {
                using (CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Read))
                {
                    cs.Read(decryptBytes, 0, decryptBytes.Length);
                    cs.Close();
                    ms.Close();
                }
            }
            return Encoding.UTF8.GetString(decryptBytes).Replace("\0", "");   ///将字符串后尾的'\0'去掉
        }

        /// <summary>
        /// AES加密算法
        /// </summary>
        /// <param name="plainText">明文字符串</param>
        /// <returns>将加密后的密文转换为Base64编码，以便显示</returns>
        public static string AESEncryptHT(string plainText, string Key)
        {
            //    plainText = "357485036443895";//357485036443895
            Key = "wineworldapp2015";
            byte[] keyArray = UTF8Encoding.UTF8.GetBytes(Key);
            byte[] toEncryptArray = UTF8Encoding.UTF8.GetBytes(plainText);
            RijndaelManaged rDel = new RijndaelManaged();
            rDel.Key = keyArray;
            rDel.IV = _key1;
            rDel.Mode = CipherMode.ECB;
            rDel.Padding = PaddingMode.Zeros;

            ICryptoTransform cTransform = rDel.CreateEncryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

            return Convert.ToBase64String(resultArray, 0, resultArray.Length);
        }

        /// <summary>
        /// AES解密
        /// </summary>
        /// <param name="cipherText">密文字符串</param>
        /// <returns>返回解密后的明文字符串</returns>
        public static string AESDecryptHT(string showText, string Key)
        {
            //showText = "kuvXd301VzjyEm69FXvfnA==";// "l6DAtbdCkv0PZ2qMuHKftg==";
            //Key = "wineworldapp2015";
            byte[] keyArray = UTF8Encoding.UTF8.GetBytes(Key);

            byte[] toEncryptArray = Convert.FromBase64String(showText);

            RijndaelManaged rDel = new RijndaelManaged();
            rDel.Key = keyArray;
            rDel.IV = _key1;
            rDel.Mode = CipherMode.ECB;
            rDel.Padding = PaddingMode.Zeros;

            ICryptoTransform cTransform = rDel.CreateDecryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

            return UTF8Encoding.UTF8.GetString(resultArray).Replace("\0", "");   ///将字符串后尾的'\0'去掉;
        }
    }

    public class MD5Helper
    {
        public static string MD5Encrypt(string plain)
        {
            MD5CryptoServiceProvider x = new MD5CryptoServiceProvider();

            byte[] bs = System.Text.Encoding.UTF8.GetBytes(plain);

            bs = x.ComputeHash(bs);

            StringBuilder s = new StringBuilder();

            foreach (byte b in bs)
            {
                s.Append(b.ToString("x2").ToLower());
            }
            return s.ToString().ToUpper();
        }
        /// <summary>
        /// MD5加密（中民返利网文档方法）
        /// </summary>
        /// <param name="strSoure">加密字符串</param>
        /// <returns></returns>
        //public static string DoubleMD5sign(string strSoure)
        //{
        //    string strSign = System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(strSoure, "MD5").ToUpper();
        //    return System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(strSign, "MD5").ToLower();
        //}
    }

    /// <summary>
    /// 导出模板类型
    /// </summary>
    public enum OutPutType:short
    {
        B2C = 1,
        B2CDetail =2,
        B2B = 3,
        B2BDetail =4,
    }
}
