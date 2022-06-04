# open-starcraft2-instance
星际2实例启动器

一键双开甚至多开星际2的工具，原理参考：https://ngabbs.com/read.php?tid=14958561&rand=997

手动从 ProcessExplorer 中寻找 Handle 并关闭比较麻烦，所以做了这样一个自动工具。

参考项目：

https://stackoverflow.com/questions/6808831/delete-a-mutex-from-another-process （遍历速度很慢，效率低，后面没有采用）

https://github.com/urosjovanovic/MceController/blob/master/VmcServices/DetectOpenFiles.cs （代码主要参考该项目，做了些调整）
