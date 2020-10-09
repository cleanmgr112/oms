//------------------------------------------------------------------------------------- 
// CMB Confidential 
// 
// Copyright (C) 2015 China Merchants Bank Co., Ltd. All rights reserved. 
// 
// No part of this file may be reproduced or transmitted in any form or by any means,  
// electronic, mechanical, photocopying, recording, or otherwise, without prior   
// written permission of China Merchants Bank Co., Ltd. 
// 
//-------------------------------------------------------------------------------------
namespace CmblifeOpenSDK
{
   static class Constants
    {
        public const string CMBLIFE_DEFAULT_PROCOTOL_PREFIX = "cmblife://";

        public const string CMBLIFE_DEFAULT_CHARSET_ENCODING = "UTF-8";

        public const string CMBLIFE_SIGN_ALGORITHM_SHA256 = "SHA256WithRSA";

        public const string CMBLIFE_SIGN_ALGORITHM_SHA1 = "SHA1WithRSA";
        
        public const string CMBLIFE_DATE_FORMAT = "yyyyMMddHHmmss";

        public const int CMBLIFE_DEFAULT_RSA_KEY_SIZE = 2048;

        public const int CMBLIFE_DEFAULT_AES_KEY_SIZE = 128;

        public const string MERCHANT_XML_PRI_KEY = "<RSAKeyValue><Modulus>gxnXvvQN0Rha0U1dpjYDujpaNkidZ3yitptvf1HtStv1wCd5qTG7LU68XJ77GenG42f+iMYfMlsnSMD+z9bH3CE91lq8DIsXGwc23khXefvdB0n9rQD3UNvDf3IaAEg1KZuQgRfAhUEoPMf6RTiTs5NXjxzqkzmSmDC24WsPbqR5OJYrubg00gcsgyWt13Ko9/PtA6xxwnADHP0+1Rju0lpUyT/3tikk3zaxfWPQCBtqJ3MzPthKZhtkkcG4dvPEgdkk45Ap8Vs1+RSbwbBCfbC7rweF3gWQwclasb1I31m0wMnKe42S2qqbOxzrexb/+EA1QDYLy6WswzwCFiHNZQ==</Modulus><Exponent>AQAB</Exponent><P>t2ZTsmlzNi8NfaXem013pkhzth+4+knjNuj7wQ3IQRdKoSOlXeNt5OknyFJW+r8hwnvdqm1LBZoi9dgHuQe5gQAmZnZ1c4k4EhQbEZn+OzD3HCu44kb17SCAb76fCIAec/pgwduzy3iYFISHOzDGkqmRyB7BEOArE7y8j6l+ydM=</P><Q>tv+TAWAweAgYPho+E6Nil0KGnh40vTdE7F3wWKJuNZFOoAyOtMGpZVDou6/c9Q4jNFvgfda2IWhYcB6p+bteBiM8nk82bcB4B7UgEcHfe2uwivpm+c2GE8pkTREjQaOae2q81OODK65SjrB5KRck3Bu0xpLKqJceIlzwQ5UhkOc=</Q><DP>kX7Ed43gsOOzODMW3u4eNfTUl2+jOCzV7QH4d7ePXtQziJLW5h0/WZL+1JU+G7718WyC9mmuUsttYMv5lHjkWcrcq/zeQMJjkTQSJWydnCfEYrzs99aD05MtUXlQgVXi0u+XQzQg9xK808ov3m1bm46a8MA1OkYc5pOco/w7cbE=</DP><DQ>n+DP5W7NScEAtRkmTO/8zdwAUppfR0THQZ6cwkM++Cv4PlpaP0/HGE5E1t1BtRNh7HesvSReQPex9FrF8/ofnksgxcq86cwy0cELwJfaETE3r2QvnWVTE21Kjg4/+DPgXp0VVwVib0JAvIsvf5fJy0ele6t5xSsp16nM+66KAdc=</DQ><InverseQ>iwH3HXiSPIMZESfb4hx5a6oOJ4eHlGCTRUpTvKvfZOallaHZOyuXX+bE3JKXvtbgvRo4SQnv1MKiHO/YvgAeYOtJlheda2jgzKndSjeCd2Cb7EwZkqRDMP7HltOKvuChiU1lua2O0AHrjF3grYtsvn6vy87KeXY0N4iq77fxAzs=</InverseQ><D>XYHZOAGquTC91ftwiFSOZA2qun0gh+eFxukmpZExxusMZXnCdMNb1f0KrKVYRCtSCHDsQ3HMXoZVhrbhCC0RcBjlmjYtWmT6nfSPVgwTGJZkGAbWQMcnnyygTA5+LSVThdHHR8xBLMpEgNXB1A1+i97T3OerCEdQH+zfA/jwkOEJnyw4R6zcNSwG2ZVY+fBM7vU32hMnldhF9tFTxPMi8eUcrVSmlX4+jPymA+8p5819gaSHqFI6k5xSZRcQejDkNLPcRHXGvJB3Eknybe22h1OKqftSRsUvKv70/XGgrwMEwVYZsIO1moeZyAIhDIZOkiUufyNMhY9Ns7LEnkFeIQ==</D></RSAKeyValue>";

        public const string CMBLIFE_XML_PUB_KEY = "<RSAKeyValue><Modulus>qoUJLtUCJSNe/XjBWci5JD9muP1e8Jbwe5c6H6oRcsD8CvY7EqZVD2GLW/UWumgtM652LQF2U3DX5Zi57XPFEjvOdRpVROaurvYVyvpudMBF3Yu33PV7k7OB+bR5cGfuUu0QOFUSJSLVTA7SPAGVP1XoIPZ48utX1jBcW9y7atVba7Mwmjhn+chDplJXQhbA3htpfRRL6ZyGQltyym6UN3dIddYxXfaL05zwbEOVwDU1H5u0QOO2chaFFyoUFEkMSRWcVUz20pbEQ0HEYUy4bPvxuLWTGauuMRAF4Ltta2uELveWHZL7USfwqFZ2Lo61x78BheicZT/fC6oZTxpfhQ==</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";
    }

}
