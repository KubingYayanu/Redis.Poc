# Redis.Poc

測試 Redis/RedLock.Net 對於併發更新/新增的特性

## Standalone

### Docker

- 執行 Docker Compose

```bash
$ docker-compose -f docker-compose.yaml -p redis-poc up -d

# 重新編譯相依服務 Image
$ docker-compose -f docker-compose.yaml -p redis-poc up -d --build
```