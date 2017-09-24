namespace PowerSupplyNotifier.Code
{
    public class ComponentLicense
    {
        public string Title { get; }
        
        public string Description { get; }

        public ComponentLicense(string title, string description)
        {
            Title = title;
            Description = description;
        }
    }
}
