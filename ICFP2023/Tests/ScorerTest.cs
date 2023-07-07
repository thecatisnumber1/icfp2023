﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Formats.Asn1.AsnWriter;

namespace Tests
{
    [TestClass]
    public class ScorerTest
    {
        [TestMethod]
        public void TestCProb1()
        {
            var prob = ProblemCatalog.Instance.GetSpec("problem-1");
            var solution = new Solution(prob);

            List<Point> points = new List<Point> { new Point(874.4230f, 61.9528f), new Point(945.4255f, 436.8367f), new Point(699.8305f, 38.8607f), new Point(625.8548f, 23.3870f), new Point(775.9987f, 432.7217f), new Point(79.4933f, 158.1369f), new Point(1037.9369f, 289.6646f), new Point(839.7457f, 347.2799f), new Point(569.4352f, 564.7177f), new Point(871.3176f, 292.1211f), new Point(695.0443f, 502.2012f), new Point(11.3235f, 103.9866f), new Point(660.9894f, 198.5462f), new Point(274.0380f, 424.0996f), new Point(778.2244f, 557.7911f), new Point(879.0146f, 18.5554f), new Point(256.7234f, 244.2711f), new Point(1048.3605f, 67.8367f), new Point(61.5665f, 166.3083f), new Point(790.5385f, 231.4158f), new Point(1119.7588f, 205.6449f), new Point(827.8003f, 428.3644f), new Point(300.3922f, 137.5732f), new Point(223.2328f, 540.1195f), new Point(844.5614f, 565.3400f), new Point(865.4817f, 22.8905f), new Point(1119.3939f, 240.7366f), new Point(39.1030f, 577.1699f), new Point(959.0288f, 159.8251f), new Point(419.4974f, 93.2665f), new Point(683.3519f, 584.2180f), new Point(916.1777f, 195.7361f), new Point(689.2505f, 548.9438f), new Point(318.1548f, 489.5935f), new Point(655.1318f, 419.8998f), new Point(920.1236f, 538.2727f), new Point(896.9890f, 497.0873f), new Point(244.7736f, 508.1645f), new Point(64.3421f, 527.6094f), new Point(285.5244f, 237.6529f), new Point(879.5994f, 520.0823f), new Point(644.1614f, 412.6121f), new Point(1026.5906f, 181.8258f), new Point(895.7431f, 42.8769f), new Point(874.3124f, 560.4294f), new Point(797.5998f, 146.7092f), new Point(565.6618f, 357.1258f), new Point(346.9964f, 20.6935f), new Point(954.6616f, 331.6311f), new Point(944.8225f, 50.5804f), new Point(725.9746f, 124.1876f), new Point(252.4655f, 391.5677f), new Point(916.5949f, 340.6213f), new Point(628.7664f, 139.0472f), new Point(31.9184f, 424.6917f), new Point(718.8055f, 115.1283f), new Point(737.0787f, 210.3348f), new Point(904.8789f, 510.4488f), new Point(370.9787f, 45.2729f), new Point(958.1354f, 367.0734f), new Point(67.7816f, 292.1868f), new Point(826.1199f, 376.9823f), new Point(1025.6201f, 538.5103f), new Point(679.5312f, 306.9160f), new Point(358.1276f, 13.8349f), new Point(356.8883f, 543.1099f), new Point(296.1759f, 186.6161f), new Point(134.1493f, 472.6621f), new Point(1020.9206f, 500.7151f), new Point(281.1592f, 17.4476f), new Point(569.5485f, 372.3252f), new Point(1020.9271f, 382.8810f), new Point(754.7606f, 77.3675f), new Point(834.8735f, 568.2589f), new Point(20.7608f, 84.2382f), new Point(953.8853f, 585.8156f), new Point(45.1739f, 287.0968f), new Point(797.9078f, 429.8265f), new Point(764.4597f, 25.9469f), new Point(750.3568f, 477.4145f), new Point(563.0736f, 82.1575f), new Point(177.1767f, 65.7588f), new Point(854.1431f, 185.3945f), new Point(484.1689f, 109.8080f), new Point(937.9338f, 163.6603f), new Point(162.2479f, 45.6390f), new Point(1026.9708f, 137.4296f), new Point(1056.9894f, 492.3603f), new Point(904.7418f, 134.0951f), new Point(315.3566f, 578.9157f), new Point(30.9030f, 83.6678f), new Point(499.1731f, 100.5780f), new Point(900.0219f, 554.4451f), new Point(335.6741f, 73.4400f), new Point(1094.9469f, 373.1761f), new Point(1091.4786f, 510.5559f), new Point(156.5960f, 222.5609f), new Point(818.8795f, 552.5047f), new Point(810.3879f, 382.4385f), new Point(781.4658f, 106.1014f), new Point(616.0847f, 110.0147f), new Point(316.0616f, 418.2976f), new Point(458.0020f, 171.4216f), new Point(180.0327f, 302.3322f), new Point(388.3508f, 277.4102f), new Point(998.2976f, 323.1650f), new Point(474.2853f, 378.0241f), new Point(323.4859f, 510.4016f), new Point(521.6925f, 584.3813f), new Point(382.7750f, 378.5265f), new Point(529.2201f, 186.6037f), new Point(604.5109f, 451.5348f), new Point(596.9851f, 432.0485f), new Point(600.9595f, 85.4103f), new Point(942.8169f, 396.6800f), new Point(578.6230f, 327.2544f), new Point(338.6914f, 313.6348f), new Point(314.1184f, 464.8192f), new Point(387.6352f, 334.3856f), new Point(130.6422f, 146.9164f), new Point(553.8655f, 241.4200f), new Point(658.6897f, 175.5144f), new Point(66.8840f, 380.7089f), new Point(350.8948f, 93.9968f), new Point(185.5096f, 396.8801f), new Point(836.4424f, 494.3788f), new Point(540.8236f, 307.7144f), new Point(252.5241f, 193.8079f), new Point(1087.6018f, 557.6946f), new Point(252.2570f, 589.9488f), new Point(718.7030f, 163.2182f), new Point(824.2652f, 535.8514f), new Point(1105.3460f, 383.6948f), new Point(871.0292f, 82.8228f), new Point(487.3251f, 477.3309f), new Point(540.8597f, 379.8975f), new Point(409.3551f, 216.3761f), new Point(831.6531f, 45.4048f), new Point(1117.0764f, 577.4535f), new Point(447.7033f, 541.1074f), new Point(747.7045f, 415.4474f), new Point(668.4310f, 366.4112f), new Point(960.3303f, 528.9799f), new Point(1076.4464f, 37.8561f), new Point(1088.0225f, 214.5695f), new Point(915.0187f, 166.8245f), new Point(1083.8401f, 130.7849f), new Point(613.2321f, 307.2036f), new Point(407.7203f, 322.4653f), new Point(1148.0820f, 81.0796f), new Point(1115.7466f, 270.6003f), new Point(204.5594f, 201.6950f), new Point(790.1340f, 185.4555f), new Point(972.1132f, 341.6203f), new Point(560.1639f, 264.4135f), new Point(101.2505f, 422.5095f), new Point(898.0901f, 259.4342f), new Point(558.5909f, 45.9488f), new Point(415.6272f, 456.7433f), new Point(155.7106f, 363.0709f), new Point(542.7682f, 140.6083f), new Point(930.1539f, 300.5815f), new Point(849.8533f, 584.2078f), new Point(44.8803f, 472.0448f), new Point(59.6631f, 476.4910f), new Point(917.9096f, 467.6646f), new Point(993.5985f, 357.6240f), new Point(549.8648f, 590.7524f), new Point(683.5235f, 371.7813f), new Point(752.1987f, 47.9695f), new Point(402.7491f, 496.2074f), new Point(1136.0994f, 585.2606f), new Point(697.8914f, 380.8069f), new Point(708.3185f, 499.0007f), new Point(704.6820f, 446.1491f), new Point(167.1580f, 468.5328f), new Point(707.9420f, 85.6059f), new Point(1049.7113f, 418.6021f), new Point(483.8797f, 38.7700f), new Point(364.5960f, 75.9740f), new Point(1025.5864f, 115.8229f), new Point(285.2245f, 263.6946f), new Point(1079.0441f, 390.4649f), new Point(234.7765f, 76.2676f), new Point(291.7205f, 532.6590f), new Point(224.8028f, 361.3787f), new Point(245.2999f, 471.2144f), new Point(987.9545f, 371.5352f), new Point(87.3736f, 564.2239f), new Point(960.2591f, 278.7752f), new Point(1073.0146f, 571.7216f), new Point(857.3798f, 324.4593f), new Point(84.5390f, 395.5216f), new Point(595.0309f, 379.6273f), new Point(760.7033f, 223.0643f), new Point(881.5468f, 204.4496f), new Point(1064.2160f, 194.3249f), new Point(887.9878f, 69.8131f), new Point(1054.9841f, 43.2750f), new Point(215.2837f, 194.7963f), new Point(1039.7306f, 202.6636f), new Point(967.0461f, 287.2430f), new Point(210.3087f, 263.1480f), new Point(992.6289f, 477.4356f), new Point(578.7366f, 435.7460f), new Point(526.2650f, 273.1434f), new Point(691.5956f, 187.8819f), new Point(1135.2488f, 423.7296f), new Point(312.7860f, 567.8987f), new Point(1028.4102f, 432.9505f), new Point(791.4376f, 481.8075f), new Point(80.5584f, 118.1286f), new Point(30.7471f, 326.6865f), new Point(636.3789f, 592.9165f), new Point(159.3669f, 139.5670f), new Point(93.4121f, 93.1997f), new Point(398.7694f, 127.8497f), new Point(892.0456f, 487.4587f), new Point(1140.4901f, 89.8860f), new Point(848.9601f, 450.8570f), new Point(353.0689f, 144.7601f), new Point(415.3576f, 137.6179f), new Point(424.2390f, 113.3717f), new Point(262.1049f, 440.7735f), new Point(120.7342f, 437.0016f), new Point(1070.8282f, 219.8867f), new Point(734.3768f, 394.0451f), new Point(121.6640f, 334.0586f), new Point(637.5302f, 13.9753f), new Point(1041.8003f, 558.4423f), new Point(397.4832f, 455.3707f), new Point(881.6126f, 478.2514f), new Point(701.4689f, 535.3178f), new Point(424.8735f, 334.1002f), new Point(605.1266f, 21.4955f), new Point(1131.8136f, 481.4221f), new Point(653.1331f, 363.5469f), new Point(253.7975f, 30.1340f), new Point(1042.9739f, 178.3410f), new Point(169.6104f, 108.9480f), new Point(709.2830f, 271.0061f), new Point(314.6561f, 73.0245f), new Point(761.1783f, 177.2358f), new Point(684.1323f, 478.4526f), new Point(146.8685f, 49.3839f), new Point(812.8298f, 282.6698f), new Point(999.7017f, 288.0645f), new Point(234.7594f, 554.6016f), new Point(68.6542f, 82.4449f), new Point(945.5170f, 552.0947f), new Point(789.7565f, 512.5779f), new Point(904.8950f, 455.1736f), new Point(408.9139f, 417.9708f), new Point(370.9439f, 421.8907f), new Point(357.5182f, 503.9974f), new Point(1101.3854f, 221.8440f), new Point(299.5656f, 210.0441f), new Point(1129.3524f, 307.9582f), new Point(263.5560f, 335.4615f), new Point(927.3509f, 584.4611f), new Point(931.1553f, 544.9551f), new Point(411.6085f, 540.4241f), new Point(816.4601f, 396.2200f), new Point(403.2025f, 109.7856f), new Point(97.8046f, 133.8801f), new Point(40.3597f, 73.2706f), new Point(211.1177f, 417.9789f), new Point(867.6814f, 223.1308f), new Point(290.0625f, 216.3316f), new Point(822.9807f, 575.0454f), new Point(705.0654f, 324.5027f), new Point(172.1866f, 262.9719f), new Point(715.9254f, 300.2245f), new Point(354.0071f, 346.0303f), new Point(1116.4694f, 82.3883f), new Point(946.5615f, 237.7037f), new Point(624.0735f, 466.9551f), new Point(462.6438f, 392.8864f), new Point(58.4010f, 178.7984f), new Point(139.3683f, 572.6887f), new Point(327.1781f, 291.8668f), new Point(715.0270f, 193.0994f), new Point(912.6452f, 18.7864f), new Point(615.9493f, 474.2911f), new Point(760.5344f, 93.1029f), new Point(768.2857f, 85.0285f), new Point(979.2126f, 214.1932f), new Point(694.1117f, 84.0552f), new Point(816.0225f, 147.3352f), new Point(1055.0413f, 141.9035f), new Point(1004.6684f, 500.3375f), new Point(805.0662f, 349.9488f), new Point(393.7290f, 288.1525f), new Point(94.6458f, 281.4001f), new Point(652.2502f, 523.7158f), new Point(462.6098f, 569.1611f), new Point(601.1122f, 524.4031f), new Point(578.9036f, 452.8324f), new Point(1077.9072f, 261.3381f), new Point(253.1368f, 53.1332f), new Point(39.0883f, 372.7584f), new Point(849.6259f, 506.9107f), new Point(500.4557f, 48.2287f), new Point(447.5261f, 39.3454f), new Point(570.0439f, 296.2279f), new Point(283.3890f, 307.5469f), new Point(892.7299f, 570.2537f), new Point(30.4295f, 225.3045f), new Point(16.1564f, 369.8195f), new Point(998.9942f, 258.6037f), new Point(521.3480f, 481.8504f), new Point(609.1877f, 584.3626f), new Point(464.1356f, 296.2264f), new Point(134.6620f, 381.7801f), new Point(1067.6543f, 99.7975f), new Point(448.9942f, 184.6758f), new Point(366.9703f, 138.2578f), new Point(629.9430f, 172.1765f), new Point(535.0880f, 206.3976f), new Point(156.9887f, 249.5320f), new Point(808.9259f, 189.4420f), new Point(700.3259f, 172.7521f), new Point(222.8118f, 475.3155f), new Point(775.3604f, 314.7860f), new Point(1054.6432f, 55.2124f), new Point(101.2040f, 164.3685f), new Point(525.5668f, 439.6407f), new Point(89.6304f, 513.4290f), new Point(1070.5251f, 590.5518f), new Point(883.3704f, 377.2690f), new Point(941.5107f, 471.4229f), new Point(61.8087f, 140.9002f), new Point(743.3444f, 159.9060f), new Point(55.5078f, 436.5634f), new Point(419.0548f, 479.2791f), new Point(496.4336f, 446.0109f), new Point(981.7573f, 277.2824f), new Point(178.6964f, 341.7893f), new Point(945.1163f, 294.1915f), new Point(675.3366f, 551.3038f), new Point(1130.8984f, 54.4655f), new Point(814.8347f, 302.7412f), new Point(571.5469f, 477.0195f), new Point(552.0449f, 507.5124f), new Point(490.2150f, 556.4787f), new Point(1081.3286f, 150.0414f), new Point(978.4851f, 62.0446f), new Point(791.2059f, 97.1850f), new Point(573.6263f, 188.3413f), new Point(618.2234f, 434.1211f), new Point(342.7975f, 198.0401f), new Point(687.1114f, 425.9403f), new Point(976.0251f, 78.0914f), new Point(168.3957f, 435.6296f), new Point(141.2383f, 164.0414f), new Point(482.0376f, 230.3804f), new Point(1049.8300f, 350.2905f), new Point(695.6012f, 268.5836f), new Point(292.5491f, 344.4394f), new Point(810.8463f, 483.0788f), new Point(150.3902f, 412.8564f), new Point(118.2454f, 466.9217f), new Point(641.4827f, 450.4678f), new Point(333.6009f, 394.2946f), new Point(1133.0160f, 389.6451f), new Point(22.6721f, 218.2806f), new Point(60.8192f, 453.4933f), new Point(571.2004f, 391.7744f), new Point(1035.2308f, 67.5725f), new Point(1058.7052f, 205.5714f), new Point(898.8392f, 83.5536f), new Point(1163.1969f, 143.7887f), new Point(889.1465f, 139.6353f), new Point(134.9616f, 229.8204f), new Point(612.1807f, 168.3464f), new Point(865.5438f, 236.0929f), new Point(643.4449f, 371.2661f), new Point(176.1437f, 352.0300f), new Point(96.2181f, 363.6221f), new Point(1100.1827f, 210.9953f), new Point(408.6301f, 13.7856f), new Point(378.2155f, 482.7319f), new Point(18.0527f, 444.4081f), new Point(578.2627f, 133.6899f), new Point(1043.8372f, 459.4676f), new Point(384.8664f, 455.7996f), new Point(509.1507f, 75.3096f), new Point(291.1638f, 471.0714f), new Point(774.4436f, 296.6525f), new Point(184.2471f, 569.0247f), new Point(300.6828f, 293.2096f), new Point(799.5864f, 409.7756f), new Point(458.3976f, 406.2151f), new Point(778.0034f, 280.0997f), new Point(442.1492f, 196.6206f), new Point(527.5649f, 65.5932f), new Point(199.5425f, 305.6683f), new Point(606.6436f, 344.1083f), new Point(315.7677f, 363.9167f), new Point(942.3777f, 189.2346f), new Point(1043.2391f, 491.7089f), new Point(203.1366f, 24.0104f), new Point(544.2066f, 437.7706f), new Point(575.9974f, 543.0146f), new Point(427.6140f, 58.5681f), new Point(866.2647f, 70.1732f), new Point(326.5428f, 325.5558f), new Point(217.1942f, 454.6371f), new Point(703.4547f, 292.8707f), new Point(94.3744f, 114.4389f), new Point(338.5889f, 91.8233f), new Point(215.7375f, 31.8399f), new Point(215.1632f, 53.8611f), new Point(122.8803f, 490.3999f), new Point(241.3171f, 351.7357f), new Point(169.6254f, 89.4687f), new Point(811.9027f, 156.4873f), new Point(933.4445f, 35.9260f), new Point(951.7770f, 127.3132f), new Point(80.1433f, 191.8947f), new Point(493.1834f, 252.1693f), new Point(50.5811f, 324.8263f), new Point(1095.7750f, 526.0977f), new Point(898.7600f, 539.5707f), new Point(1011.1952f, 167.2231f), new Point(19.3991f, 514.1864f), new Point(668.7029f, 506.5452f), new Point(488.1103f, 568.9224f), new Point(1059.6860f, 68.3400f), new Point(101.7960f, 585.4719f), new Point(317.6015f, 241.7634f), new Point(1126.1201f, 163.1945f), new Point(186.7092f, 354.5002f), new Point(499.6388f, 537.9890f), new Point(517.6130f, 546.4100f), new Point(175.6926f, 141.8398f), new Point(485.4196f, 218.7306f), new Point(122.6085f, 409.0339f), new Point(633.4844f, 491.6853f), new Point(912.0482f, 237.2543f), new Point(722.5530f, 517.7328f), new Point(573.7631f, 46.4604f), new Point(457.4157f, 152.9458f), new Point(215.0244f, 431.0398f), new Point(909.5364f, 77.4454f), new Point(170.0811f, 249.8963f), new Point(146.1825f, 60.7863f), new Point(854.5866f, 219.2103f), new Point(80.9880f, 441.8450f), new Point(605.0934f, 156.1751f), new Point(890.1857f, 582.8783f), new Point(354.5771f, 83.8806f), new Point(56.2379f, 512.8657f), new Point(53.4899f, 410.9269f), new Point(585.1337f, 293.1989f), new Point(1089.4846f, 232.6397f), new Point(544.0332f, 364.6170f), new Point(729.3835f, 30.0292f), new Point(765.2393f, 494.3017f), new Point(214.3016f, 283.4387f), new Point(383.4339f, 101.5203f), new Point(510.0065f, 161.1739f), new Point(589.6195f, 92.7890f), new Point(335.6854f, 110.5360f), new Point(891.0369f, 518.9858f), new Point(265.8310f, 63.2620f), new Point(675.2615f, 108.5949f), new Point(793.5546f, 375.9134f), new Point(341.3708f, 11.3149f), new Point(675.6041f, 175.8563f), new Point(152.2300f, 70.1128f), new Point(472.0389f, 100.4754f), new Point(672.1445f, 400.7789f), new Point(1062.1431f, 300.7093f), new Point(63.9664f, 210.2281f), new Point(966.8068f, 121.0662f), new Point(559.6202f, 14.3176f), new Point(346.7717f, 442.2552f), new Point(105.5090f, 147.2080f), new Point(1148.3326f, 235.3011f), new Point(810.5146f, 570.1161f), new Point(1072.5411f, 51.7753f), new Point(407.7886f, 198.8184f), new Point(931.2560f, 378.6710f), new Point(841.0485f, 553.5570f), new Point(1028.1012f, 58.0629f), new Point(896.0972f, 594.0244f), new Point(306.5721f, 202.2686f), new Point(691.7219f, 297.0421f), new Point(543.9559f, 405.9217f), new Point(225.2030f, 515.1237f), new Point(1055.2216f, 371.2725f), new Point(221.4224f, 146.2181f), new Point(67.0981f, 63.9139f), new Point(123.7317f, 198.7928f), new Point(738.6086f, 428.9613f), new Point(771.3741f, 519.7987f), new Point(311.5562f, 186.3271f), new Point(551.7176f, 531.1034f), new Point(272.4902f, 341.2610f), new Point(657.7776f, 439.9965f), new Point(989.8349f, 488.7064f), new Point(818.3673f, 129.5306f), new Point(514.1547f, 563.1789f), new Point(440.9060f, 465.3852f), new Point(713.4326f, 431.5401f), new Point(773.0120f, 354.1856f), new Point(582.0738f, 532.6943f), new Point(783.0875f, 355.1439f), new Point(742.8950f, 594.2794f), new Point(1000.6067f, 69.5313f), new Point(848.2840f, 264.5126f), new Point(800.8647f, 443.1335f), new Point(1060.4293f, 236.9313f), new Point(552.9218f, 319.0603f), new Point(237.3117f, 446.5194f), new Point(978.9031f, 319.1327f), new Point(771.8482f, 477.9968f), new Point(39.2924f, 554.9242f), new Point(726.7532f, 491.6219f), new Point(396.7399f, 424.5017f), new Point(425.6702f, 270.5154f), new Point(1165.4087f, 422.2420f), new Point(439.0790f, 145.4880f), new Point(356.4096f, 390.0557f), new Point(393.3487f, 395.8075f), new Point(193.7902f, 115.6805f), new Point(248.6517f, 326.6611f), new Point(669.4075f, 530.6978f), new Point(427.3734f, 28.6738f), new Point(442.9035f, 222.3051f), new Point(714.6468f, 142.0399f), new Point(271.0509f, 495.1992f), new Point(483.9491f, 350.2201f), new Point(1155.0491f, 271.9140f), new Point(399.7432f, 522.9047f), new Point(32.8548f, 110.9143f), new Point(1060.5712f, 350.6788f), new Point(182.3348f, 594.8929f), new Point(1157.3615f, 502.0896f), new Point(416.7125f, 26.5413f), new Point(494.7513f, 280.6555f), new Point(753.8218f, 293.4649f), new Point(73.6580f, 355.5304f), new Point(519.5084f, 402.6527f), new Point(884.1669f, 31.7042f), new Point(993.8889f, 386.8446f), new Point(942.3313f, 94.6532f), new Point(727.1703f, 66.3746f), new Point(162.8536f, 71.4758f), new Point(147.6492f, 383.4871f), new Point(515.4796f, 454.2016f), new Point(132.5490f, 65.9556f), new Point(765.1351f, 335.2981f), new Point(534.0978f, 426.7049f), new Point(928.1386f, 45.9944f), new Point(1164.9332f, 247.2676f), new Point(66.6381f, 418.5781f), new Point(49.3761f, 352.9015f), new Point(436.0582f, 86.4558f), new Point(594.2264f, 268.1923f), new Point(269.2969f, 469.3163f), new Point(574.9089f, 205.3932f), new Point(1067.4342f, 114.0142f), new Point(685.2589f, 358.9620f), new Point(38.1059f, 172.9084f), new Point(260.7208f, 231.0722f), new Point(953.0375f, 391.1183f), new Point(164.4741f, 33.2130f), new Point(1111.9021f, 22.6868f), new Point(1014.2589f, 36.2919f), new Point(703.2945f, 64.6986f), new Point(464.6227f, 453.2051f), new Point(791.0280f, 121.1521f), new Point(534.2433f, 366.9098f), new Point(335.6096f, 234.2727f), new Point(295.6849f, 576.2626f), new Point(268.2141f, 545.7234f), new Point(100.9700f, 195.4226f), new Point(460.4353f, 140.9107f), new Point(655.9611f, 500.1948f), new Point(1161.5659f, 463.3283f), new Point(48.9513f, 298.7495f), new Point(640.0200f, 347.8242f), new Point(724.8848f, 423.6903f), new Point(1057.2491f, 391.6516f), new Point(517.2406f, 139.6856f), new Point(794.1944f, 221.6990f), new Point(705.7328f, 334.8710f), new Point(458.2276f, 20.4817f), new Point(1007.7178f, 156.1091f), new Point(11.4373f, 298.0493f), new Point(431.1852f, 431.9649f), new Point(891.5980f, 476.6854f), new Point(513.5435f, 352.2433f), new Point(312.1070f, 269.4789f), new Point(352.2113f, 570.7264f), new Point(450.8647f, 520.2895f), new Point(673.2034f, 276.8746f), new Point(616.9969f, 507.2556f), new Point(58.7668f, 569.9863f), new Point(1112.4033f, 114.1206f), new Point(333.9395f, 364.3958f), new Point(680.3645f, 211.0505f), new Point(182.8470f, 433.7002f), new Point(966.5857f, 562.7736f), new Point(113.9174f, 531.8677f), new Point(744.5738f, 192.1630f), new Point(756.4642f, 102.4011f), new Point(626.8835f, 216.0373f), new Point(852.6450f, 365.9751f), new Point(139.2062f, 485.6531f), new Point(302.5308f, 383.1564f), new Point(920.3105f, 215.8046f), new Point(752.8165f, 205.2250f), new Point(824.6470f, 586.2814f), new Point(161.4617f, 503.4841f), new Point(63.9753f, 369.2560f), new Point(102.3721f, 400.2718f), new Point(846.4228f, 409.2377f), new Point(517.0403f, 330.2176f), new Point(98.3779f, 181.7471f), new Point(925.5323f, 553.4889f), new Point(1103.1656f, 284.6665f), new Point(329.3119f, 280.5410f), new Point(518.1404f, 368.5656f), new Point(1157.6475f, 28.0851f), new Point(720.7543f, 368.5085f), new Point(838.8505f, 220.2071f), new Point(24.1055f, 66.3185f), new Point(890.2559f, 463.3003f), new Point(454.7735f, 131.1198f), new Point(779.1472f, 491.5981f), new Point(258.3096f, 277.4384f), new Point(1061.0165f, 477.6569f), new Point(510.3379f, 434.9454f), new Point(509.8244f, 501.3406f), new Point(1025.0078f, 81.3279f), new Point(376.5113f, 30.4952f), new Point(794.3284f, 202.8034f), new Point(962.2766f, 439.2070f), new Point(384.2653f, 363.0092f), new Point(385.9604f, 235.7930f), new Point(932.2535f, 341.2812f), new Point(217.1221f, 582.1202f), new Point(571.1398f, 70.0047f), new Point(193.9996f, 186.7313f), new Point(1080.1349f, 403.6828f), new Point(366.3852f, 62.4265f), new Point(931.4402f, 237.6995f), new Point(329.4135f, 180.9320f), new Point(267.6291f, 288.1712f), new Point(809.7997f, 258.7133f), new Point(313.4674f, 537.1304f), new Point(136.5017f, 592.1880f), new Point(1120.1342f, 254.0098f), new Point(539.3214f, 153.5750f), new Point(627.4913f, 453.6618f), new Point(409.8771f, 80.2966f), new Point(753.0618f, 151.2869f), new Point(783.9973f, 369.0526f), new Point(10.9100f, 75.1676f), new Point(25.9728f, 119.0915f), new Point(891.3754f, 279.8614f), new Point(339.0918f, 336.0107f), new Point(685.0074f, 335.3896f), new Point(843.1597f, 325.9525f), new Point(1163.7148f, 45.4961f), new Point(754.4998f, 446.0748f), new Point(258.4375f, 85.2127f), new Point(556.1212f, 557.3502f), new Point(540.3424f, 52.2767f), new Point(783.7867f, 129.1740f), new Point(1023.2568f, 102.5754f), new Point(1028.8066f, 322.4480f), new Point(778.7059f, 468.7829f), new Point(251.2291f, 462.7987f), new Point(545.2505f, 99.3165f), new Point(958.3778f, 208.7744f), new Point(821.9077f, 348.4468f), new Point(872.8361f, 331.6902f), new Point(989.1340f, 416.9090f), new Point(968.3637f, 467.6122f), new Point(716.0032f, 351.9182f), new Point(499.9323f, 466.3347f), new Point(242.9427f, 108.8110f), new Point(41.3013f, 451.5127f), new Point(459.6396f, 431.9488f), new Point(960.9526f, 571.5970f), new Point(762.3227f, 139.9991f), new Point(1039.3630f, 359.3760f), new Point(1023.6432f, 260.0229f), new Point(730.6320f, 451.1608f), new Point(296.1812f, 430.9148f), new Point(758.9172f, 466.2873f), new Point(160.8532f, 566.4697f), new Point(118.1457f, 229.3301f), new Point(345.4830f, 372.4602f), new Point(739.4633f, 458.6657f), new Point(811.6708f, 426.9014f), new Point(41.3239f, 402.9482f), new Point(280.4882f, 449.6933f), new Point(1110.6257f, 403.3352f), new Point(556.0710f, 119.5270f), new Point(711.1776f, 58.1648f), new Point(520.2165f, 160.9060f), new Point(61.2848f, 396.9790f), new Point(1137.8840f, 176.1474f), new Point(959.1003f, 552.2817f), new Point(339.2509f, 244.8297f), new Point(454.2202f, 532.8101f), new Point(425.7269f, 245.9147f), new Point(197.1851f, 467.6986f), new Point(1144.0958f, 188.2334f), new Point(443.6874f, 111.7735f), new Point(98.3097f, 256.4809f), new Point(252.0585f, 572.0026f), new Point(748.5275f, 216.6747f), new Point(419.2861f, 522.2289f), new Point(1123.4722f, 151.5277f), new Point(987.4266f, 182.4069f), new Point(117.4619f, 358.7027f), new Point(498.5498f, 383.8041f), new Point(666.3401f, 252.3274f), new Point(406.6469f, 483.8048f), new Point(1068.7490f, 343.4395f), new Point(523.4621f, 91.3866f), new Point(1085.1866f, 22.5668f), new Point(164.7238f, 303.8490f), new Point(394.4606f, 313.5030f), new Point(524.5852f, 108.9627f), new Point(238.2218f, 197.0889f), new Point(977.4598f, 479.1648f), new Point(547.6666f, 224.1249f), new Point(175.8059f, 500.8230f), new Point(361.3938f, 360.5873f), new Point(1120.5614f, 352.3713f), new Point(960.0147f, 61.4481f), new Point(987.2100f, 547.7130f), new Point(816.0369f, 63.6444f), new Point(319.9667f, 402.0749f), new Point(323.9580f, 223.2083f), new Point(219.6767f, 498.3645f), new Point(836.6625f, 454.0929f), new Point(279.3925f, 539.8757f), new Point(25.2396f, 477.4743f), new Point(650.1138f, 48.9155f), new Point(829.2396f, 215.7921f), new Point(565.7106f, 503.9807f), new Point(485.9272f, 460.1348f), new Point(96.1423f, 487.2687f), new Point(729.1762f, 54.8079f), new Point(314.6801f, 10.1866f), new Point(234.2130f, 591.3550f), new Point(671.3231f, 163.3396f), new Point(54.9156f, 102.4864f), new Point(631.2871f, 519.4202f), new Point(604.6183f, 287.7879f), new Point(102.7144f, 244.7661f), new Point(219.8000f, 20.8622f), new Point(1055.5576f, 166.5107f), new Point(300.9474f, 172.6297f), new Point(487.0523f, 493.4034f), new Point(495.7508f, 218.5987f), new Point(893.5469f, 353.3142f), new Point(688.3525f, 220.4991f), new Point(921.1032f, 90.1097f), new Point(805.3860f, 179.3374f), new Point(253.6079f, 540.4000f), new Point(797.0295f, 284.2431f), new Point(750.6236f, 120.7142f), new Point(90.7738f, 35.2955f), new Point(763.8125f, 197.4446f), new Point(915.6242f, 361.5047f), new Point(962.7914f, 254.4860f), new Point(206.5782f, 347.1620f), new Point(712.3236f, 244.2724f), new Point(828.7769f, 77.1884f), new Point(223.5997f, 526.3525f), new Point(388.7819f, 533.6379f), new Point(942.1995f, 202.5853f), new Point(195.3750f, 293.3749f), new Point(666.4909f, 67.5646f), new Point(1164.6217f, 366.2606f), new Point(856.5607f, 486.4659f), new Point(145.1196f, 39.0815f), new Point(261.8867f, 130.7920f), new Point(113.7486f, 547.9232f), new Point(1160.6569f, 62.5004f), new Point(735.4805f, 97.8039f), new Point(289.6511f, 441.2510f), new Point(434.5751f, 529.6779f), new Point(847.1041f, 198.1634f), new Point(912.8069f, 32.4394f), new Point(310.5773f, 33.9527f), new Point(448.9095f, 555.9735f), new Point(452.8169f, 477.8625f), new Point(256.7848f, 39.8780f), new Point(467.0456f, 43.9654f), new Point(436.4593f, 162.8327f), new Point(1011.1157f, 251.8738f), new Point(925.0346f, 480.0708f), new Point(496.8588f, 573.8129f), new Point(77.0538f, 495.5082f), new Point(557.5382f, 393.4900f), new Point(331.1595f, 51.5848f), new Point(1060.5479f, 461.9187f), new Point(584.6843f, 553.7326f), new Point(393.4011f, 384.7512f), new Point(327.8996f, 166.8084f), new Point(857.2461f, 306.8321f), new Point(160.2332f, 294.3074f), new Point(205.7637f, 484.3783f), new Point(537.3550f, 350.1063f), new Point(983.2769f, 123.6040f), new Point(1099.7188f, 493.2316f), new Point(591.2637f, 470.0505f), new Point(986.0275f, 459.3750f), new Point(358.3571f, 202.9042f), new Point(432.3879f, 412.1773f), new Point(589.9506f, 66.9489f), new Point(1053.1704f, 93.9025f), new Point(261.3920f, 153.3939f), new Point(856.5933f, 461.4218f), new Point(864.7559f, 33.2379f), new Point(131.2969f, 548.2484f), new Point(961.2278f, 224.8122f), new Point(915.7604f, 329.8771f), new Point(1131.2999f, 291.1527f), new Point(1159.9629f, 565.6126f), new Point(656.8173f, 210.2994f), new Point(710.7758f, 516.6681f), new Point(1155.8717f, 519.7527f), new Point(676.3199f, 229.7912f), new Point(84.3354f, 264.2388f), new Point(324.2527f, 433.7786f), new Point(1003.5259f, 420.1050f), new Point(850.1894f, 37.3217f), new Point(666.8454f, 183.3929f), new Point(550.0377f, 518.5900f), new Point(819.8242f, 478.1053f), new Point(893.9675f, 384.7745f), new Point(808.5708f, 414.1805f), new Point(929.6995f, 390.6026f), new Point(910.2640f, 109.7697f), new Point(379.6414f, 207.6508f), new Point(796.6378f, 545.5399f), new Point(1045.3254f, 85.0104f), new Point(313.7546f, 88.5390f), new Point(627.1262f, 250.2341f), new Point(600.0327f, 175.1327f), new Point(918.9193f, 429.4789f), new Point(720.8141f, 37.0246f), new Point(85.0188f, 54.5774f), new Point(113.0814f, 559.9908f), new Point(934.5666f, 322.5707f), new Point(596.1821f, 327.9443f), new Point(367.1232f, 273.8421f), new Point(1015.9446f, 592.1493f), new Point(871.6072f, 43.3987f), new Point(179.2153f, 209.6035f), new Point(689.4580f, 232.1247f), new Point(385.4362f, 151.6239f), new Point(809.7460f, 138.9072f), new Point(1138.5875f, 435.2078f), new Point(1055.6511f, 14.0366f), new Point(445.9586f, 355.2126f), new Point(204.0081f, 127.9524f), new Point(189.4672f, 495.4686f), new Point(266.4076f, 182.9311f), new Point(811.6592f, 12.3220f), new Point(175.3446f, 534.5106f), new Point(936.0762f, 355.0889f), new Point(52.5118f, 486.0236f), new Point(142.1405f, 339.0285f), new Point(1165.8141f, 298.1687f), new Point(625.3160f, 117.1407f), new Point(1030.5793f, 393.4336f), new Point(188.5821f, 381.0226f), new Point(96.4428f, 312.2819f), new Point(365.4414f, 151.3145f), new Point(187.5382f, 175.2571f), new Point(665.1078f, 467.7408f), new Point(388.5403f, 475.9957f), new Point(173.8422f, 322.1630f), new Point(506.6826f, 85.9510f), new Point(608.2862f, 277.8100f), new Point(73.2496f, 332.6352f), new Point(839.2711f, 172.7730f), new Point(379.8722f, 567.2007f), new Point(682.6049f, 503.8328f), new Point(264.8929f, 75.5810f), new Point(737.6891f, 373.1033f), new Point(1068.4140f, 531.9630f), new Point(689.3423f, 386.9603f), new Point(1143.0568f, 51.3868f), new Point(949.9351f, 412.3974f), new Point(1023.9636f, 512.9928f), new Point(1051.1567f, 432.2419f), new Point(555.5771f, 181.2257f), new Point(524.2557f, 256.3137f), new Point(474.2918f, 274.1424f), new Point(727.4291f, 225.6440f), new Point(1159.0779f, 133.3161f), new Point(685.6647f, 23.1311f), new Point(266.0544f, 308.0663f), new Point(264.7613f, 511.2619f), new Point(859.9562f, 262.9300f), new Point(339.9486f, 574.1741f), new Point(196.2245f, 433.0982f), new Point(111.2080f, 484.2226f), new Point(277.0377f, 354.7332f), new Point(437.7821f, 269.3017f), new Point(24.2924f, 399.1801f), new Point(769.4312f, 161.8881f), new Point(472.0016f, 365.4241f), new Point(1123.1742f, 547.2929f), new Point(237.9504f, 324.1679f), new Point(764.4680f, 566.9656f), new Point(378.9106f, 507.9925f), new Point(383.6446f, 66.8209f), new Point(763.1014f, 113.2900f), new Point(1031.7151f, 223.8287f), new Point(434.1516f, 329.6911f), new Point(673.2476f, 453.9551f), new Point(635.6107f, 437.7572f), new Point(920.0753f, 316.3445f), new Point(751.5447f, 60.4912f), new Point(656.0964f, 460.2936f), new Point(1013.9484f, 50.1873f), new Point(517.2732f, 121.2601f), new Point(385.2856f, 412.9603f), new Point(901.4468f, 391.6370f), new Point(446.4687f, 24.4764f), new Point(913.1524f, 583.3275f), new Point(124.3641f, 528.9954f), new Point(138.4434f, 264.3650f), new Point(519.6898f, 381.1545f), new Point(568.9766f, 109.6806f), new Point(1130.9250f, 190.5640f), new Point(1097.9396f, 336.3002f), new Point(355.4402f, 243.2614f), new Point(929.4812f, 208.4030f), new Point(141.0820f, 455.1726f), new Point(138.3611f, 128.8178f), new Point(634.4838f, 62.4784f), new Point(789.1562f, 34.1398f), new Point(936.9028f, 524.5182f), new Point(67.6956f, 559.7594f), new Point(285.7950f, 414.1298f), new Point(720.5246f, 578.3608f), new Point(362.3313f, 412.5575f), new Point(525.2826f, 417.6391f), new Point(762.8932f, 384.6791f), new Point(593.7222f, 512.0069f), new Point(781.1295f, 571.8086f), new Point(1076.2820f, 426.8343f), new Point(71.9296f, 95.9402f), new Point(408.6942f, 50.6591f), new Point(754.1005f, 492.0494f), new Point(116.7646f, 54.3384f), new Point(1114.4524f, 591.7367f), new Point(796.9533f, 557.7030f), new Point(561.0699f, 540.7144f), new Point(414.7826f, 268.2520f), new Point(429.7534f, 468.2611f), new Point(501.5837f, 133.6065f), new Point(280.4081f, 41.6689f), new Point(754.0110f, 34.1085f), new Point(904.3567f, 423.3231f), new Point(390.5361f, 206.2399f), new Point(378.9913f, 125.7318f), new Point(666.7294f, 307.5193f), new Point(122.6679f, 274.3156f), new Point(110.6993f, 518.7730f), new Point(1074.1980f, 436.8283f), new Point(698.3740f, 563.7737f), new Point(497.1279f, 13.2750f), new Point(194.6874f, 153.7985f), new Point(162.9134f, 424.7425f), new Point(1124.9546f, 559.9091f), new Point(1134.3452f, 404.9715f), new Point(870.7195f, 313.8290f), new Point(308.8627f, 525.9871f), new Point(1094.1774f, 398.3456f), new Point(1008.0023f, 228.7596f), new Point(1105.0986f, 61.9616f), new Point(536.9909f, 503.0706f), new Point(267.1518f, 567.2875f), new Point(659.9784f, 140.7997f), new Point(41.6383f, 484.2268f), new Point(503.5587f, 35.7729f), new Point(221.8975f, 259.7456f), new Point(607.6576f, 411.3080f), new Point(1073.1289f, 293.1280f), new Point(511.2820f, 262.8161f), new Point(188.3033f, 94.7387f), new Point(1031.8845f, 561.5551f), new Point(607.9968f, 102.6157f), new Point(397.9068f, 260.3750f), new Point(637.8677f, 229.2143f), new Point(188.8250f, 457.8440f), new Point(608.6164f, 515.5104f), new Point(212.6832f, 132.9331f), new Point(342.6309f, 161.7789f), new Point(567.7128f, 171.1827f), new Point(449.8212f, 439.6231f), new Point(194.9923f, 449.8635f), new Point(952.8173f, 305.5814f), new Point(1067.7015f, 19.1740f), new Point(1119.3923f, 512.9047f), new Point(805.2197f, 326.5297f), new Point(637.4290f, 293.6316f), new Point(498.7552f, 78.2735f), new Point(929.1221f, 406.2693f), new Point(146.4610f, 187.2547f), new Point(1150.3024f, 404.4746f), new Point(995.4931f, 147.5120f), new Point(281.5618f, 555.4252f), new Point(404.3731f, 229.6764f), new Point(76.8527f, 511.3255f), new Point(1103.4470f, 186.6524f), new Point(784.5021f, 55.1546f), new Point(513.1218f, 96.2488f), new Point(500.5510f, 152.2812f), new Point(520.0961f, 224.1522f), new Point(127.7507f, 120.9890f), new Point(1139.2532f, 457.8171f), new Point(537.0656f, 392.6641f), new Point(718.3019f, 313.7603f), new Point(168.6909f, 333.9462f), new Point(365.4023f, 461.3320f), new Point(483.6162f, 173.3513f), new Point(470.1680f, 353.9713f), new Point(144.9500f, 207.7191f), new Point(371.6373f, 179.3033f), new Point(85.0721f, 486.7552f), new Point(112.2118f, 103.9498f), new Point(204.6348f, 567.6602f), new Point(807.9444f, 545.0475f), new Point(831.5453f, 58.3545f), new Point(779.3229f, 174.7465f), new Point(480.3970f, 147.3305f), new Point(198.1256f, 407.0655f), new Point(799.1600f, 468.4605f), new Point(410.1839f, 161.1622f), new Point(152.0143f, 306.9978f), new Point(645.5049f, 121.8970f), new Point(974.1152f, 500.5067f), new Point(651.6717f, 228.1225f), new Point(564.8228f, 316.6373f), new Point(1084.1925f, 323.3126f), new Point(279.9909f, 210.0288f), new Point(413.9152f, 346.5249f), new Point(967.9584f, 133.1634f), new Point(337.0129f, 289.7273f), new Point(528.3507f, 133.5702f), new Point(548.2086f, 299.6610f), new Point(925.7983f, 518.5819f) };
            for (int i = 0; i < points.Count; i++)
            {
                solution.SetPlacement(prob.Musicians[i], points[i]);
            }

            var score = solution.ComputeScore();
            Assert.AreEqual(9227687, score);
        }

        [TestMethod]
        public void TestExampleProblem()
        {
            var prob = ProblemSpec.ReadJson(ProblemSpecTest.PROBLEM);
            var solution = new Solution(prob);

            List<Point> points = new List<Point> { new Point(590.0000f, 10.0000f), new Point(1100.0000f, 100.0000f), new Point(1100.0000f, 150.0000f) };
            for (int i = 0; i < points.Count; i++)
            {
                solution.SetPlacement(prob.Musicians[i], points[i]);
            }


            var score = solution.ComputeScore();
            Assert.AreEqual(5343, score);
            
        }
    }
}
