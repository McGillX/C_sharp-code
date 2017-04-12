using System.Runtime.Serialization;

namespace ConsoleApplication2
{
    [DataContract]
    public class DragAndDropLoad
    {
        //context has userid and courseid
        [DataMember(Name = "context")]
        public Context context { get; set; }

        [DataMember(Name = "time")]
        public string time { get; set; }
        [DataMember(Name = "event_type")]
        public string event_type { get; set; }

        public override string ToString()
        {
            return context.ToString() + " " + event_type + " " + time;
        }
    }

    [DataContract]
    public class ItemDropEvent
    {
        [DataMember(Name = "item_id")]
        public int id { get; set; }
        [DataMember(Name = "item")]
        public string id_name { get; set; }

        [DataMember(Name ="is_correct")]
        public bool correctness { get; set; }

        [DataMember(Name ="location")]
        public string loc_name { get; set; }
        [DataMember(Name = "location_id")]
        public string loc_id { get; set; }

        public override string ToString()
        {
            return id + " " + id_name + " " + correctness + " " + loc_name + " " + loc_id;
        }
    }

    [DataContract]
    public class DragAndDropItemDropped
    {
        //context has userid and courseid
        [DataMember(Name = "context")]
        public Context context { get; set; }

        [DataMember(Name = "time")]
        public string time { get; set; }
        [DataMember(Name = "event_type")]
        public string event_type { get; set; }

        [DataMember(Name = "event")]
        public ItemDropEvent dropEvent { get; set; }

        public override string ToString()
        {
            // return context.ToString() + " " + event_type + " " + time + " " + dropEvent.id;
            return event_type;
        }
    }

    [DataContract]
    public class ItemLiftEvent
    {
        [DataMember(Name = "item_id")]
        public int id { get; set; }
    }

    [DataContract]
    public class DragAndDropItemLifted
    {
        //context has userid and courseid
        [DataMember(Name = "context")]
        public Context context { get; set; }

        [DataMember(Name = "time")]
        public string time { get; set; }
        [DataMember(Name = "event_type")]
        public string event_type { get; set; }

        [DataMember(Name = "event")]
        public ItemLiftEvent liftEvent { get; set; }

        public override string ToString()
        {
            return context.ToString() + " " + event_type + " " + time + " " + liftEvent.id;
        }
    }
}
