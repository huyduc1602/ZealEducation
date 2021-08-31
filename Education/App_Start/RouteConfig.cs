using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Education
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.MapRoute(
            name: "faqs",
            url: "Faqs",
            defaults: new { controller = "Home", action = "FAQS", slug = UrlParameter.Optional },
            namespaces: new[] { "Education.Controllers" }
            );

            routes.MapRoute(
            name: "blogs",
            url: "Blogs",
            defaults: new { controller = "Home", action = "Blogs", slug = UrlParameter.Optional },
            namespaces: new[] { "Education.Controllers" }
            );
            routes.MapRoute(
          name: "contact",
          url: "Contact",
          defaults: new { controller = "Home", action = "Contact", slug = UrlParameter.Optional },
          namespaces: new[] { "Education.Controllers" }
          );
            routes.MapRoute(
          name: "about",
          url: "About",
          defaults: new { controller = "Home", action = "About", slug = UrlParameter.Optional },
          namespaces: new[] { "Education.Controllers" }
      );
            routes.MapRoute(
              name: "courses",
              url: "Courses",
              defaults: new { controller = "Home", action = "Courses", slug = UrlParameter.Optional },
              namespaces: new[] { "Education.Controllers" }
          );

            routes.MapRoute(
               name: "course_slug",
               url: "Courses/{slug}",
               defaults: new { controller = "Courses", action = "Details", slug = UrlParameter.Optional },
               namespaces: new[] { "Education.Controllers" }
           );

            routes.MapRoute(
               name: "blog_slug",
               url: "Blogs/{slug}",
               defaults: new { controller = "Blogs", action = "Details", slug = UrlParameter.Optional },
               namespaces: new[] { "Education.Controllers" }
           );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional },
                namespaces: new[] { "Education.Controllers" }
            );

        }
    }
}

