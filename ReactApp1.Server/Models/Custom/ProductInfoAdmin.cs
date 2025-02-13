﻿namespace ReactApp1.Server.Models.Custom
{
    public class ProductInfoAdmin
    {
        public int? ProductId { get; set; }

        public int? ProductItemId { get; set; }

        public string? CategoryName { get; set; }

        public int? CategoryId { get; set; }

        public string? Name { get; set; }

        public double? Price { get; set; }

        public string? Description { get; set; } = "";

        public Dictionary<string, string>? Characteristics { get; set; } = new Dictionary<string, string>();
        public string? Sku { get; set; }

        public int? QtyInStock { get; set; }

        public string? ImageUrl { get; set; } = "default.jpg";

        public List<string>? ImagesUrl { get; set; } = ["default.jpg",];



    }
}
