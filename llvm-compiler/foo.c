#include <stdio.h>
#include <stdlib.h>
struct boo{
 int bx;
};
int f(int x)
{
    
}

struct my_string
{
	int len;
	char* value;
};

struct my_string my_string_concat(struct my_string* left, struct my_string* right)
{
	struct my_string result;
	result.len = left->len + right->len;
	result.value = malloc(result.len);
	sprintf(result.value, "%.*s", left->len, left->value);
	sprintf(result.value + left->len, "%.*s", right->len, right->value);
	return result;
}


struct my_string to_string(int num)
{
	int sum = num;
	int count = 0;
	while (sum >= 10)
	{
		sum = sum / 10;
		count++;
	}
	count++;
	
	char* buffer = malloc(count);
	sprintf(buffer, "%d", num);
	struct my_string str;
	str.len = count;
	str.value = buffer;
	
    return str;
}

int main()
{
	int num = 100;
	struct my_string x = to_string(num);
	struct my_string y = to_string(200);
	struct my_string z = my_string_concat(&x, &y);
	printf("%.*s", z.len, z.value);
	
	
	
	
		
	
    
    return 0;
}

