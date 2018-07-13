namespace MetromTablet.Models
{
    public class OptionsModel
    {

        public bool TowMode { get; set; }
        public TowFrom TowFrom { get; set; }
        public float TowOffset { get; set; }
        public LoadProfile LoadProfile { get; set; }
        public string ActiveProfile { get; set; }


        public OptionsModel()
        {
            TowMode = false;
            TowFrom = TowFrom.Rear;
            TowOffset = 10;
            LoadProfile = LoadProfile.Indoors;
            ActiveProfile = string.Empty;
        }


        public OptionsModel(OptionsModel model)
        {
            TowMode = model.TowMode;
            TowFrom = model.TowFrom;
            TowOffset = model.TowOffset;
            LoadProfile = model.LoadProfile;
            ActiveProfile = model.ActiveProfile;
        }
    }


    public enum TowFrom { Rear, Front };
    public enum LoadProfile { Indoors, Outdoors };
}
