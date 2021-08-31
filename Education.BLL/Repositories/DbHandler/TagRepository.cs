using Education.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Education.BLL.Repositories.DbHandler
{
    public class TagRepository : DbRepository<Tag>
    {
        public List<Tag> AddTags(string tags)
        {
            List<Tag> tagList = new List<Tag>();
            string[] TagContent = tags.Split(',');
            foreach (var item in TagContent)
            {
                var tag = tbl.FirstOrDefault(x => x.Content.ToLower().Equals(item.ToLower()));
                if (tag != null)
                {
                    tagList.Add(tag);
                }
                else
                {
                    Tag tagNew = new Tag();
                    tagNew.Content = item;
                    tbl.Add(tagNew);
                    context.SaveChanges();
                    tagList.Add(tagNew);
                }
                
            }
            return tagList;
        }
       
    }
}
