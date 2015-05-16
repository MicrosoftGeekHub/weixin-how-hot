from requests import post, codes
from time import time
import xml.etree.ElementTree as etree


target_url = "http://msftgeekblog.azurewebsites.net/"

test_url_prefix ="http://geeekstore.blob.core.windows.net/cdn/weiTestCase/"
test_file_list = [
    "book1.jpg",
    "book2.jpg",
    "book3.jpg",
    "book4.jpg",
    "build1.jpg",
    "build2.jpg",
    "build3.jpg",
    "coupl2.jpg",
    "couple1.jpg",
    "female1.jpg",
    "female2.jpg",
    "female3.jpg",
    "food1.jpg",
    "food10.jpg",
    "food11.jpg",
    "food2.jpg",
    "food3.jpg",
    "food4.jpg",
    "food5.jpg",
    "food6.jpg",
    "food7.jpg",
    "food8.jpg",
    "food9.jpg",
    "group0.jpg",
    "group1.jpg",
    "group2.jpg",
    "group3.jpg",
    "group4.jpg",
    "keyboard.jpg",
    "male1.jpg",
    "male2.jpg",
    "male3.jpg",
    "male4.jpg",
    "manual.jpg",
    "menu1.jpg",
    "menu2.jpg",
    "menu3.jpg",
    "menu4.jpg",
    "menu5.jpg",
    "menu6.jpg",
    "outdoor1.jpg",
    "outdoor2.jpg",
    "outdoor4.jpg",
    "png-menu.png",
    "post1.jpg",
    "post2.jpg",
    "qr.jpg",
    "railway.jpg",
    "screen-male.jpg",
    "screen1.jpg",
    "screen2.jpg",
    "screen3.jpg",
    "screen4.jpg",
    "sky.jpg",
    "venue1.jpg",
    "venue2.jpg",
    "venue3.jpg",
]

post_content_template = """
<xml>
<ToUserName><![CDATA[toUnitTest]]></ToUserName>
<FromUserName><![CDATA[fromUnitTest]]></FromUserName>
<CreateTime>%d</CreateTime>
<MsgType><![CDATA[image]]></MsgType>
<PicUrl><![CDATA[%s]]></PicUrl>
<MediaId><![CDATA[media_id]]></MediaId>
<MsgId>1234567890</MsgId>
</xml>
"""


def post_image(url):
    try:
        headers = {'Content-Type': 'application/xml'}
        resp = post(target_url, data = post_content_template % (int(time()), url), headers = headers, timeout = 30)
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
for index, filename in enumerate(test_file_list):
    url = test_url_prefix + filename
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
    print("#", index + 1, "-", url, valid, time_interval, "!" * max(0, int(time_interval - 4)))

count_all = count_ok + count_fail
print("{} test in total, ok: {}, fail: {}, success rate: {:.2f}%, average secs: {:.4f}".format(count_all, count_ok, count_fail, count_ok / count_all * 100, total_time / count_all))
