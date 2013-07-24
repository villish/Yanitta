
namespace Yanitta
{
    public class RotationItem
    {
        public Profile Profile   { get; private set; }
        public Rotation Rotation { get; private set; }
        public bool IsEmpty      { get; private set; }

        public RotationItem() 
        {
            this.Profile  = default(Profile);
            this.Rotation = default(Rotation);
            this.IsEmpty  = true;
        }

        public RotationItem(Profile Profile, Rotation Rotation)
        {
            this.Profile  = Profile;
            this.Rotation = Rotation;
            this.IsEmpty  = false;
        }

        public override string ToString()
        {
            if (IsEmpty) 
                return string.Empty;

            return string.Format("{0}: {1}", this.Profile, this.Rotation);
        }
    }
}
