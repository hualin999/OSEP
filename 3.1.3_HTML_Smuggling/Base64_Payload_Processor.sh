#!/bin/bash

# Step 1: 对文件进行Base64编码
base64_output=$(base64 /var/www/html/msfstaged.exe)

# Step 2: 对生成的Base64编码进行进一步处理
# 删除换行符、新行，将内容合并为一个连续字符串
processed_payload=$(echo "$base64_output" | tr -d '\n\r')

# 或者，将每行内容用引号括起来
# quoted_payload=$(echo "$base64_output" | sed 's/.*/"&"/')

# 打印处理后的Payload
echo "$processed_payload"