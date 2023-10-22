#!/bin/bash
# 透過迴圈嘗試建立 redis User, 避免 redis server 尚未完成啟動就呼叫 redis-cli

acl_folder="/data/conf/"
acl_file_path="/data/conf/users.acl"

# 檢查 /data/conf/users.acl 是否不存在
if [ ! -e $acl_file_path ]; then
    # 建立 users.acl
    mkdir $acl_folder
    echo "" > $acl_file_path
    chmod 777 $acl_folder && chmod 777 $acl_file_path

    while :
    do
        # 使用 quit 嘗試中斷連線, 測試 redis server 是否啟動
        sleep 1
        redis-cli quit
        if [ $? -eq 0 ]; then
            echo "Server ready now, start to init users..."
                # $REDIS_INITUSER_USERNAME 不為空白，透過 $REDIS_INITUSER_USERNAME(環境變數) 取得新增使用者帳號與密碼                
                if [ -n "$REDIS_INITUSER_USERNAME" ]; then
                    # 跳脫字元 \> 避免設定密碼失效
                    redis-cli ACL SETUSER $REDIS_INITUSER_USERNAME \>$REDIS_INITUSER_PASSWORD on ~* +@all
                fi
                # 儲存 ACL 到 users.acl
                redis-cli ACL SAVE
                # 關閉預設使用者, 避免被無權限者竄改資料
                # redis-cli ACL SETUSER default off
                # 登入新增的使用者
                # redis-cli AUTH $REDIS_INITUSER_USERNAME $REDIS_INITUSER_PASSWORD
                # 顯示 ACL 使用者列表
                # redis-cli ACL LIST
            break;
        else
            echo "Server not ready, wait then retry..."
            sleep 3
        fi
    done
fi