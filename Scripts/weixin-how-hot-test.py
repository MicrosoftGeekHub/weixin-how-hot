from requests import post, codes
from time import time
import xml.etree.ElementTree as etree


target_url = "http://msftgeekblog.azurewebsites.net/"

test_url_list = [
    "http://geeekstore.blob.core.windows.net/cdn/weiTestCase/book1.jpg",
    "http://geeekstore.blob.core.windows.net/cdn/weiTestCase/book2.jpg",
    "http://geeekstore.blob.core.windows.net/cdn/weiTestCase/book3.jpg",
    "http://geeekstore.blob.core.windows.net/cdn/weiTestCase/book4.jpg",
    "http://geeekstore.blob.core.windows.net/cdn/weiTestCase/build1.jpg",
    "http://geeekstore.blob.core.windows.net/cdn/weiTestCase/build2.jpg",
    "http://geeekstore.blob.core.windows.net/cdn/weiTestCase/build3.jpg",
    "http://geeekstore.blob.core.windows.net/cdn/weiTestCase/coupl2.jpg",
    "http://geeekstore.blob.core.windows.net/cdn/weiTestCase/couple1.jpg",
    "http://geeekstore.blob.core.windows.net/cdn/weiTestCase/female1.jpg",
    "http://geeekstore.blob.core.windows.net/cdn/weiTestCase/female2.jpg",
    "http://geeekstore.blob.core.windows.net/cdn/weiTestCase/female3.jpg",
    "http://geeekstore.blob.core.windows.net/cdn/weiTestCase/food1.jpg",
    "http://geeekstore.blob.core.windows.net/cdn/weiTestCase/food10.jpg",
    "http://geeekstore.blob.core.windows.net/cdn/weiTestCase/food11.jpg",
    "http://geeekstore.blob.core.windows.net/cdn/weiTestCase/food2.jpg",
    "http://geeekstore.blob.core.windows.net/cdn/weiTestCase/food3.jpg",
    "http://geeekstore.blob.core.windows.net/cdn/weiTestCase/food4.jpg",
    "http://geeekstore.blob.core.windows.net/cdn/weiTestCase/food5.jpg",
    "http://geeekstore.blob.core.windows.net/cdn/weiTestCase/food6.jpg",
    "http://geeekstore.blob.core.windows.net/cdn/weiTestCase/food7.jpg",
    "http://geeekstore.blob.core.windows.net/cdn/weiTestCase/food8.jpg",
    "http://geeekstore.blob.core.windows.net/cdn/weiTestCase/food9.jpg",
    "http://geeekstore.blob.core.windows.net/cdn/weiTestCase/group0.jpg",
    "http://geeekstore.blob.core.windows.net/cdn/weiTestCase/group1.jpg",
    "http://geeekstore.blob.core.windows.net/cdn/weiTestCase/group2.jpg",
    "http://geeekstore.blob.core.windows.net/cdn/weiTestCase/group3.jpg",
    "http://geeekstore.blob.core.windows.net/cdn/weiTestCase/group4.jpg",
    "http://geeekstore.blob.core.windows.net/cdn/weiTestCase/keyboard.jpg",
    "http://geeekstore.blob.core.windows.net/cdn/weiTestCase/male1.jpg",
    "http://geeekstore.blob.core.windows.net/cdn/weiTestCase/male2.jpg",
    "http://geeekstore.blob.core.windows.net/cdn/weiTestCase/male3.jpg",
    "http://geeekstore.blob.core.windows.net/cdn/weiTestCase/male4.jpg",
    "http://geeekstore.blob.core.windows.net/cdn/weiTestCase/manual.jpg",
    "http://geeekstore.blob.core.windows.net/cdn/weiTestCase/menu1.jpg",
    "http://geeekstore.blob.core.windows.net/cdn/weiTestCase/menu2.jpg",
    "http://geeekstore.blob.core.windows.net/cdn/weiTestCase/menu3.jpg",
    "http://geeekstore.blob.core.windows.net/cdn/weiTestCase/menu4.jpg",
    "http://geeekstore.blob.core.windows.net/cdn/weiTestCase/menu5.jpg",
    "http://geeekstore.blob.core.windows.net/cdn/weiTestCase/menu6.jpg",
    "http://geeekstore.blob.core.windows.net/cdn/weiTestCase/outdoor1.jpg",
    "http://geeekstore.blob.core.windows.net/cdn/weiTestCase/outdoor2.jpg",
    "http://geeekstore.blob.core.windows.net/cdn/weiTestCase/outdoor4.jpg",
    "http://geeekstore.blob.core.windows.net/cdn/weiTestCase/png-menu.png",
    "http://geeekstore.blob.core.windows.net/cdn/weiTestCase/post1.jpg",
    "http://geeekstore.blob.core.windows.net/cdn/weiTestCase/post2.jpg",
    "http://geeekstore.blob.core.windows.net/cdn/weiTestCase/qr.jpg",
    "http://geeekstore.blob.core.windows.net/cdn/weiTestCase/railway.jpg",
    "http://geeekstore.blob.core.windows.net/cdn/weiTestCase/screen-male.jpg",
    "http://geeekstore.blob.core.windows.net/cdn/weiTestCase/screen1.jpg",
    "http://geeekstore.blob.core.windows.net/cdn/weiTestCase/screen2.jpg",
    "http://geeekstore.blob.core.windows.net/cdn/weiTestCase/screen3.jpg",
    "http://geeekstore.blob.core.windows.net/cdn/weiTestCase/screen4.jpg",
    "http://geeekstore.blob.core.windows.net/cdn/weiTestCase/sky.jpg",
    "http://geeekstore.blob.core.windows.net/cdn/weiTestCase/venue1.jpg",
    "http://geeekstore.blob.core.windows.net/cdn/weiTestCase/venue2.jpg",
    "http://geeekstore.blob.core.windows.net/cdn/weiTestCase/venue3.jpg",
]

post_content_template = """
<xml>
<ToUserName><![CDATA[toUnitTest]]></ToUserName>
<FromUserName><![CDATA[fromUnitTest]]></FromUserName>
<CreateTime>1348831860</CreateTime>
<MsgType><![CDATA[image]]></MsgType>
<PicUrl><![CDATA[%s]]></PicUrl>
<MediaId><![CDATA[media_id]]></MediaId>
<MsgId>1234567890</MsgId>
</xml>
"""


def post_image(url):
    try:
        headers = {'Content-Type': 'application/xml'}
        resp = post(target_url, data = post_content_template % url, headers = headers, timeout = 20)
        if resp.status_code == codes.ok:
            return resp.text
    except Exception as e:
        print("post_image", url, e)


def parse_response_valid(text):
    is_valid = False
    try:
        tree = etree.fromstring(text)
        for child in list(tree):
            if child.tag == "Content":
                is_valid = "blob.core.windows.net/" in child.text
                break
    except Exception as e:
        print("parse_response_valid", e)
    return is_valid


count_ok = 0
count_fail = 0
total_time = 0.0
for index, url in enumerate(test_url_list):
    time_start = time()
    text = post_image(url)
    time_end = time()
    time_interval = time_end - time_start
    total_time += time_interval
    valid = parse_response_valid(text)
    if valid:
        count_ok += 1
    else:
        count_fail += 1
    print("#", index + 1, "-", url, valid, time_interval)
    # if index == 5:
    #     break

count_all = count_ok + count_fail
print("{} test in total, ok: {}, fail: {}, success rate: {:.2f}%, average secs: {:.4f}".format(count_all, count_ok, count_fail, count_ok / count_all * 100, total_time / count_all))
