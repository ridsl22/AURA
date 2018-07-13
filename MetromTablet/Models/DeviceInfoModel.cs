
namespace MetromTablet.Models
{
    public class DeviceInfoModel
    {

        public string UIM { get; set; }
        public string CEM { get; set; }
        public string RCM { get; set; }
        public string MAC { get; set; }
        public string Name { get; set; }
        public byte GroupID { get; set; }
        public double FrontOff { get; set; }
        public double RearOff { get; set; }
		public string PM { get; set; }


        public DeviceInfoModel()
        {
			UIM = " - ";// "02.19.00";
            CEM = "02.15.00";
            RCM = "02.02.13";
            MAC = "0007e2";
            Name = "POR000002";
            GroupID = 33;
            FrontOff = 0.0;
            RearOff = 0.0;
        }


        public DeviceInfoModel(DeviceInfoModel model)
        {
            UIM = model.UIM;
            CEM = model.CEM;
            RCM = model.RCM;
            MAC = model.MAC;
            Name = model.Name;
            GroupID = model.GroupID;
            FrontOff = model.FrontOff;
            RearOff = model.RearOff;
			PM = model.PM;
        }
    }
}
