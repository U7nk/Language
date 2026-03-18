

namespace Foo
{

struct Bar
{
	public:
	bool f1;
	bool f2;
	bool f3;
	
	public:
	Bar()
	{
		this->f3 = true;
	}

	public:
	void add_two_number()
	{
		this->f1 = true;
		this->f3 = false;
	}
};

struct Babayka
{
	public:
	bool f1;
	bool f2;
	bool f3;
	
	public:
	void add_three_number(bool x)
	{
		this->f1 = x;
		this->f3 = false;
		bool k = true;
		if (x == true)
		{
			this->f3 = this->f1 | x;
			k = false;
		}
		else 
		{
			k = true;
		}
		
		this->f3 = k;
		
	}
};

};

int main()
{
    auto b = new Foo::Bar();
	//auto c = Foo::Babayka();
	b->f1 = true;
	b->add_two_number();
	//c.add_three_number(false);
    return (int)&b->f3;
}