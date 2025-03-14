using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace jotun.Models
{
	public class EventCalendarViewModel
	{
		public DateTime EventDate { get; set; }
		public string EventName { get; set; }
		public List<EventViewModel> Events { get; set; }
	}

	public class EventViewModel	
	{
		public string Name { get; set; }
		public DateTime Date { get; set; }
		public string Description { get; set; }
	}
}