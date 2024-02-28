在 GIT BASH中的操作

//配置用户基本信息
git config --global user.name <用户名>
git config --global user.email <用户邮箱>

git init  			//创建新仓库
//下载项目
git clone <远程仓库的项目链接>（[Git：远程分支----git pull和git push命令用法介绍_git pull 格式_Samven_7的博客-CSDN博客](https://blog.csdn.net/qq_42780289/article/details/97804091)）

[git的add、commit、push的详细介绍 - 简书 (jianshu.com)](https://www.jianshu.com/p/2e1d551b8261)



### 常用操作

•创建repository-->创建一个文件夹，文件的更改都会被监控

•将更改commit到本地git--> 保存文件夹此时的快照到本地的git

•恢复到某个commit--> 将文件夹恢复到之前保存的某个快照

•push和pull操作-->同步本地git和云端git的内容

•使用分支-->将当前文件夹copy一份再做更改

•常用工作流-->分支的使用方式



### 1.创建repository

使用git clone + git仓库地址, 将某个仓库下载到本地



### 2.将更改commit到本地git

常规流程：

•git status --> 查看修改的状态

•git add--> 指定此次commit包含的文件

•git commit -a –m “注释”-->完成一次到本地git的commit



### 3.恢复到某个commit   常规流程：

•git log ,查看之前的commit，版本库的状态

•git reset –-hard 版本号（取前8位即可） ,恢复到某个commit



### 4.Pull操作

#### 4.1.1 git pull命令的完整格式：

git pull <远程主机名> <远程分支名>:<本地分支名>

作用是拉取 远程主机 的指定分支，再与本地的指定分支合并。



#### 4.1.2  省略本地分支名：

 git pull <远程主机名> <远程分支名>

作用是取回远程主机的指定分支，再与本地的当前分支合并。

**例如**：git pull origin dev ，假设本地的当前分支是master。

**结果**：取回远程主机 origin 的 dev 分支与本地的当前分支，即与 master 分支合并。



#### 4.1.3 当前分支与远程分支存在追踪关系（可能有多个追踪关系）:

git pull <远程主机名>

取回远程主机与当前分支有追踪关系的分支，再与本地的当前分支合并。如果没有追踪关系使用该命令则会报错。若当前分支只有一个追踪关系，可省略 <远程主机名>。



#### 4.1.4  在本地删除远程主机上已删除的对应分支：

git pull -p

如果在远程主机删除了某个分支，默认情况下，git pull 不会在拉取远程分支的时候，删除对应的本地分支。如果要删除，加上 -p 选项即可。



### 5.Push操作

#### git 常用命令 Push博文：

https://blog.csdn.net/weixin_37646636/article/details/131455120?spm=1001.2101.3001.6650.2&utm_medium=distribute.pc_relevant.none-task-blog-2%7Edefault%7EYuanLiJiHua%7EPosition-2-131455120-blog-97804091.235%5Ev38%5Epc_relevant_sort_base2&depth_1-utm_source=distribute.pc_relevant.none-task-blog-2%7Edefault%7EYuanLiJiHua%7EPosition-2-131455120-blog-97804091.235%5Ev38%5Epc_relevant_sort_base2&utm_relevant_index=5



#### 5.1 git push命令的完整格式：

git push <远程主机名> <本地分支名>:<远程分支名>

作用是将本地的指定分支 推送 到远程主机的指定分支上。

**注意：git pull是 <远程主机名><远程分支名>:<本地分支名>，而git push是 <远程主机名><本地分支名>:<远程分支名>。**



#### 5.2 省略远程分支名

git push <远程主机名> <本地分支名>

作用是将本地的指定分支推送到远程主机的同名分支上。注意如果远程主机不存在该同名分支，则会自动新建同名分支。

**例如**：git push origin dev ，假设本地的当前分支是master，本地还有dev分支

**结果**：将本地的 dev 分支推送到远程主机 origin 的 dev 分支上，和本地的当前分支是 master 没有关系。



#### 5.3 省略本地分支名 （注意有冒号 ：）

git push <远程主机名> :<远程分支名>

如果省略本地分支名，则表示删除指定的远程分支，因为这等同于推送一个空的本地分支到远程分支



#### 5.4 当前分支与远程分支存在追踪关系（可能有多个追踪关系）

git push <远程主机名>

作用是将本地的当前分支推送到远程主机有追踪关系的对应分支上。如果没有追踪关系使用该命令则会报错。若当前分支与远程分支只有一个追踪关系，则可以省略 <远程主机名>



#### 5.5 建立追踪关系并指定默认远程主机

git push -u <远程主机名> <本地分支名>

作用是将本地的指定分支推送到远程主机的同名分支。另外因为加了-u参数，所以如果没有追踪关系则会建立，另外如果当前指定分支与多个远程主机存在追踪关系，则指定该远程主机为默认主机，后面就可以不加参数使用git push。



#### 2.6 推送所有分支

git push --all <远程主机名>

不管是否存在对应的远程分支，将本地的所有分支都推送到远程主机，这时需要使用–all选项。如果远程主机的版本比本地版本更加新，推送时Git会报错，要求先在本地做git pull合并差异或解决冲突，然后再推送到远程主机。但是如果你一定要推送，可以使用下面的–force选项。



#### 2.7 用本地所有分支覆盖远程所有分支

git push --force <远程主机名>

使用–force选项，结果导致远程主机上的版本直接被覆盖。除非你很确定要这样做，否则应该尽量避免使用–force选项。



#### 2.8 附（对标签tag的推送）

推送一个本地标签到远程： git push <远程主机名> <本地标签名>

推送本地全部未推送过的标签到远程：git push <远程主机名> --tags



删除一个远程标签（即推送一个空的标签到远程标签）：

git push <远程主机名> :refs/tags/<远程标签名>



在Git v1.7.0 之后，还可以使用下面这种语法删除远程标签：

git push <远程主机名> --delete tag <远程标签名>



### 6.使用分支

•git branch 新branch名称--> 创建新branch

•git checkout 分支名 --> 切换branch

•git merge --> 合并分支

•git branch–D--> 删除分支