var initshare=function(n,t,i,r){function v(n,t,i){n.attachEvent?(n["e"+t+i]=i,n[t+i]=function(){n["e"+t+i](window.event)},n.attachEvent("on"+t,n[t+i])):n.addEventListener(t,i,!1)}var s=document.getElementById("share_weixin"),h=document.getElementById("share_sina"),c=document.getElementById("share_qq"),l=document.getElementById("share_renren"),a=document.getElementById("share_qzone"),f=encodeURIComponent(document.location.href),t=encodeURIComponent(""+t),n=encodeURIComponent(n),i=encodeURIComponent(""+i),r=encodeURIComponent(""+r),u,y=document.getElementById("social_share"),e=document.getElementById("weixin_container"),o;h&&(u="http://service.t.sina.com.cn/share/share.php?url="+f+"&appkey=&title="+i+"&pic="+n+"&ralateUid=&searchPic=false",h.setAttribute("href",u));c&&(u="http://v.t.qq.com/share/share.php?url="+f+"&title="+i+"&pic="+n,c.setAttribute("href",u));l&&(u="http://widget.renren.com/dialog/share?resourceUrl="+f+"&title="+t+"&appkey=&pic="+n+"&content="+i+"&message="+r,l.setAttribute("href",u));a&&(u="http://sns.qzone.qq.com/cgi-bin/qzshare/cgi_qzshare_onekey?url="+f+"&title="+t+"&appkey=&pics="+n+"&summary="+i,a.setAttribute("href",u));s&&e&&(v(s,"click",function(){e.style.display="block"}),o=document.getElementById("weixin_container_close"),o&&v(o,"click",function(){e.style.display="none"}));y.style.display="block"};new initshare("http://cn.how-old.net/Images/faces2/main6_results.png","微软颜龄机器人","颜龄机器人火爆你的世界, 秀出你的颜龄","")