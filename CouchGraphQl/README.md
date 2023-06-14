## Vault Setup ##

This project is using [HashiCorp Vault](https://developer.hashicorp.com/vault) to get DB credentials.

- Install and unseal following the directions on the HashiCorp site.
- Login
  - http://localhost:8200 
- Enable UserPass
  - http://localhost:8200/ui/vault/access
  - Click "Enable new method"
  - Pick "Username & Password"
- Create User
  - In this sample, in the appsettings.json file, the username is `test`.
- Create Group
  - Pick any name that makes sense to you
  - Add the user to the group
- Create a policy to allow the group to read the secret
  - http://localhost:8200/ui/vault/policies/acl
  - Click "Create ACL Policy"
  - Add policies [see below]
  - If you create a "v2" secret engine, you will need `data` in the policy path.
- Go back to the group
  - Add the policy to the group
- Enable the secret engine
  - http://localhost:8200/ui/vault/secrets
  - Click "Enable new engine"
  - Pick "KV"
  - Set the path (e.g. `secretv2`)
  - Expand "Method Options"
  - Pick Version = 2
    - V2 enables versioning, which is helpful if someone makes a mistake
  - Click "Enable Engine"
- Add the secret
  - http://localhost:8200/ui/vault/secrets
  - Click the name you chose when enabling the engine
  - Click "Create secret"
  - Set the path (e.g. `couchbase`)
  - Toggle to JSON
    - Enter `{ "password": "REPLACE", "username": "REPLACE" }`
- Test in a console
  - Login
    - `vault login -method=userpass username=test`
  - List
    - `vault kv list secretv2`
  - Read
    - `vault kv get secretv2/couchbase`

### Sample policy ###

In the sample `secret` is a V1 KV engine, and `secret2` is a V2 KV engine.

*The code in this project uses V2.*

```shell
path "secret/*" {
    capabilities = ["list"]
}

path "secretv2/*" {
    capabilities = ["list"]
}

path "secret/couchbase" {
    capabilities = ["read", "list"]
}

path "secretv2/data/couchbase" {
    capabilities = ["read", "list"]
}
```