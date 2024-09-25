using System.ComponentModel.DataAnnotations;

namespace Yarp.Models
{
    public class ConsumerGroupModel
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
    }


    public class CreateConsumerGroupModel
    {
        [Required]
        public string Name { get; set; }
    }
 


    public class ConsumerModel
    {
        public int Id { get; set; }
        [Required]
        public string ConsumerGroupId { get; set; }
        [Required]
        public string Name { get; set; }
    }

    public class CreateConsumerModel
    {
        [Required]
        public string ConsumerGroupId { get; set; }
        [Required]
        public string Name { get; set; }
    }
    public class PatchConsumerModel
    {

        public string ConsumerGroupId { get; set; }

        public string Name { get; set; }
    }












}
