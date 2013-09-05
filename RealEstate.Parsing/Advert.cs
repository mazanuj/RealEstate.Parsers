using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RealEstate.Parsing
{
    public class AdvertHeader
    {
        public string Url { get; set; }
        public DateTime DateSite { get; set; }
    }

    public class Advert
    {
        public Advert()
        {
            Usedtype = Parsing.Usedtype.Used;
        }

        public int Id { get; set; } //+
        public string Url { get; set; } //+

        public string Title { get; set; } //+

        public Int64 Price { get; set; } //+

        public string Name { get; set; }  //+
        public string PhoneNumber { get; set; }
        public string Email { get; set; }

        public string City { get; set; } //+
        public string Distinct { get; set; }
        public string Address { get; set; } //+
        public string MetroStation { get; set; } //+

        public string MessageShort { get; set; }
        public string MessageFull { get; set; } //+

        public string Rooms { get; set; } //+

        public float AreaFull { get; set; } //+
        public float AreaLiving { get; set; }
        public float AreaKitchen { get; set; }

        public short Floor { get; set; } //+
        public short FloorTotal { get; set; } //+

        public RealEstateType RealEstateType { get; set; }
        public Usedtype Usedtype { get; set; } //+
        public AdvertType AdvertType { get; set; } //+

        public bool isGold { get; set; }

        public bool isUnique { get; set; }

        public string Tag { get; set; }

        public DateTime DateSite{ get; set; } //+
        public DateTime DateUpdate { get; set; } //+

        public virtual ICollection<Image> Images { get; set; }

        public override string ToString()
        {
            return String.Format(
                "Rooms: {0}, Area: {1:#.0#}, Floor: {2}, Floor total: {3}, Seller: {4}, "
              + "City: {5} , Adress: {6}, Metro: {7}, AdverType: {8}, UsedType: {9},Price: {10}", 
                this.Rooms, this.AreaFull, this.Floor, this.FloorTotal, this.Name, 
                this.City, this.Address, this.MetroStation, this.AdvertType, this.Usedtype ,
                this.Price );
        }

    }

    public class Image
    {
        public int Id { get; set; }
        public string URl {get;set;}
        public string LocalPath {get;set;}

        public int AdvertId { get; set; }
    }

    public enum RealEstateType
    {
        All,
        Flat,
        House
    }

    public enum Usedtype
    {
        None,
        New,
        Used
    }

    public enum AdvertType
    {
        All,
        Buy,
        Sell,
        Rent,
        Pass
    }
}
