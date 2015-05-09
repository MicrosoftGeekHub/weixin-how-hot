using Microsoft.ProjectOxford.Vision;
using System.Collections.Generic;
using System.Net;

namespace WeixinServer.Helpers
{
    /// <summary>
    /// The class is used to access vision APIs.
    /// </summary>
    public partial class VisionHelper
    {
        /// <summary>
        /// The vision service client.
        /// </summary>
        private readonly IVisionServiceClient visionClient;
        private Dictionary<string, string> cateMap = null;
        private string frameImageUri;
        private string originalImageUrl;
        private byte[] photoBytes;

        /// <summary>
        /// Initializes a new instance of the <see cref="VisionHelper"/> class.
        /// </summary>
        /// <param name="subscriptionKey">The subscription key.</param>
        public VisionHelper(string subscriptionKey, string frameImageUri)
        {
            this.visionClient = new VisionServiceClient(subscriptionKey);
            this.frameImageUri = frameImageUri;

            //this.cateMap = new Dictionary<string, string>();
            //var fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            //    "86.cate.scp.split.ts.scp");
            //using (StreamReader reader = new StreamReader(System.Web.HttpContext.Current.Server.MapPath(@"~\App_Data\86.cate.scp.split.ts.scp")))
            //{
            //    var line = reader.ReadLine();
            //    string[] sp = line.Split('\t');
            //    this.cateMap.Add(sp[0], sp[1]);
            //}
            this.cateMap = new Dictionary<string, string>() {
                { "abstract_", "抽象" },
                { "abstract_net", "带有网格的抽象" },
                { "abstract_nonphoto", "不是照片的抽象东东" },
                { "abstract_rect", "矩形的抽象" },
                { "abstract_shape", "有形的抽象" },
                { "abstract_texture", "带有纹理的抽象" },
                { "animal_", "动物" },
                { "animal_bird", "鸟" },
                { "animal_cat", "猫" },
                { "animal_dog", "狗" },
                { "animal_horse", "马" },
                { "animal_panda", "熊猫" },
                { "building_", "建筑" },
                { "building_arch", "拱" },
                { "building_brickwall", "砖墙" },
                { "building_church", "教堂" },
                { "building_corner", "墙角" },
                { "building_doorwindows", "门窗" },
                { "building_pillar", "柱子" },
                { "building_stair", "楼梯" },
                { "building_street", "街道" },
                { "dark_", "黑暗" },
                { "drink_", "饮料" },
                { "drink_can", "罐装饮料" },
                { "dark_fire", "火" },
                { "dark_fireworks", "烟花" },
                { "sky_object", "天空" },
                { "food_", "食品" },
                { "food_bread", "面包" },
                { "food_fastfood", "快餐" },
                { "food_grilled", "烤肉" },
                { "food_pizza", "比萨饼" },
                { "indoor_", "室内" },
                { "indoor_churchwindow", "教堂窗户" },
                { "indoor_court", "球场" },
                { "indoor_doorwindows", "室内门窗" },
                { "indoor_marketstore", "市场店" },
                { "indoor_room", "房间" },
                { "indoor_venue", "体育场" },
                { "dark_light", "光" },
                { "others_", "其他" },
                { "outdoor_", "户外" },
                { "outdoor_city", "城市" },
                { "outdoor_field", "农田" },
                { "outdoor_grass", "草坪" },
                { "outdoor_house", "房子" },
                { "outdoor_mountain", "山" },
                { "outdoor_oceanbeach", "海滩" },
                { "outdoor_playground", "操场" },
                { "outdoor_railway", "铁路" },
                { "outdoor_road", "马路" },
                { "outdoor_sportsfield", "运动场" },
                { "outdoor_stonerock", "岩石" },
                { "outdoor_street", "街道" },
                { "outdoor_water", "水" },
                { "outdoor_waterside", "湖滨" },
                { "people_", "人" },
                { "people_baby", "宝宝" },
                { "people_crowd", "群众" },
                { "people_group", "人群" },
                { "people_hand", "手" },
                { "people_many", "许多人" },
                { "people_portrait", "肖像" },
                { "people_show", "达人秀" },
                { "people_tattoo", "纹身" },
                { "people_young", "年轻人" },
                { "plant_", "植物" },
                { "plant_branch", "树枝" },
                { "plant_flower", "花" },
                { "plant_leaves", "叶子" },
                { "plant_tree", "树" },
                { "object_screen", "屏幕" },
                { "object_sculpture", "雕塑" },
                { "sky_cloud", "云" },
                { "sky_sun", "太阳" },
                { "people_swimming", "游泳者" },
                { "outdoor_pool", "游泳池" },
                { "text_", "文本" },
                { "text_mag", "杂志" },
                { "text_map", "地图" },
                { "text_menu", "菜单" },
                { "text_sign", "符号" },
                { "trans_bicycle", "自行车" },
                { "trans_bus", "公交" },
                { "trans_car", "汽车" },
                { "trans_trainstation", "交通工具" },
		    };
        }
    }
}
